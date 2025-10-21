using System.Text.RegularExpressions;

namespace Domain.ValueObjects
{
    // VO sellado e inmutable
    public sealed record PhoneNumber
    {
        /// <summary>
        /// Valor canónico en formato E.164, e.g. +573001112233
        /// </summary>
        public string E164 { get; init; }
        public string CountryCode { get; init; } // e.g. +57
        public string NationalNumber { get; init; } // e.g. 3001112233 (sin espacios)

        // Ctor privado para EF
        private PhoneNumber() { }

        private PhoneNumber(string e164, string countryCode, string nationalNumber)
        {
            E164 = e164;
            CountryCode = countryCode;
            NationalNumber = nationalNumber;
        }

        // Mapa simple de validaciones (pragmático). Evita chequear length aparte del patrón.
        private static readonly Dictionary<string, Regex> Patterns = new(StringComparer.Ordinal)
        {
            // CO: móviles empiezan por 3 y tienen 10 dígitos
            ["+57"] = new Regex(@"^3\d{9}$", RegexOptions.Compiled),
            // US/CA (NANP) 10 dígitos
            ["+1"] = new Regex(@"^[2-9]\d{2}\d{3}\d{4}$", RegexOptions.Compiled),
            ["+44"] = new Regex(@"^7\d{9}$", RegexOptions.Compiled),     // UK mobile
            ["+49"] = new Regex(@"^1[5-7]\d{8,9}$", RegexOptions.Compiled), // DE mobile
            ["+34"] = new Regex(@"^6\d{8}$", RegexOptions.Compiled),     // ES mobile
            ["+33"] = new Regex(@"^(6|7)\d{8}$", RegexOptions.Compiled), // FR mobile
            ["+55"] = new Regex(@"^(?:\d{2})?9\d{8}$", RegexOptions.Compiled), // BR mobile con/ sin DDD
            ["+52"] = new Regex(@"^1?\d{10}$", RegexOptions.Compiled),   // MX mobile (con o sin 1)
            ["+91"] = new Regex(@"^[789]\d{9}$", RegexOptions.Compiled), // IN
            ["+61"] = new Regex(@"^4\d{8}$", RegexOptions.Compiled),     // AU
            ["+81"] = new Regex(@"^[789]\d{9}$", RegexOptions.Compiled), // JP
            ["+7"] = new Regex(@"^9\d{9}$", RegexOptions.Compiled),     // RU
            ["+39"] = new Regex(@"^\d{9,10}$", RegexOptions.Compiled),   // IT
            ["+86"] = new Regex(@"^1[3-9]\d{9}$", RegexOptions.Compiled),// CN
            ["+82"] = new Regex(@"^1[016789]\d{7,8}$", RegexOptions.Compiled), // KR
            ["+62"] = new Regex(@"^8\d{9,10}$", RegexOptions.Compiled),  // ID
            ["+27"] = new Regex(@"^7[1-9]\d{7}$", RegexOptions.Compiled),// ZA
            ["+351"] = new Regex(@"^9[1236]\d{7}$", RegexOptions.Compiled), // PT
            ["+31"] = new Regex(@"^6\d{8}$", RegexOptions.Compiled),     // NL
            ["+63"] = new Regex(@"^9\d{9}$", RegexOptions.Compiled),     // PH
        };

        /// <summary>
        /// Crea un PhoneNumber validado y normalizado a E.164.
        /// Lanza ArgumentException con mensaje claro si hay error.
        /// </summary>
        public static PhoneNumber Create(string rawValue, string countryCode)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
                throw new ArgumentException("Phone number is required", nameof(rawValue));
            if (string.IsNullOrWhiteSpace(countryCode))
                throw new ArgumentException("Country code is required", nameof(countryCode));

            var cc = NormalizeCountryCode(countryCode);
            var national = NormalizeToDigits(rawValue);

            if (!Patterns.TryGetValue(cc, out var regex))
                throw new ArgumentException($"Unsupported country code '{countryCode}'", nameof(countryCode));

            if (!regex.IsMatch(national))
                throw new ArgumentException($"Invalid phone format for country {cc}", nameof(rawValue));

            var e164 = cc + national; // sin ceros a la izquierda adicionales; depende del patrón por país

            return new PhoneNumber(e164, cc, national);
        }

        /// <summary>
        /// Variante “segura” sin excepciones.
        /// </summary>
        public static bool TryCreate(string rawValue, string countryCode, out PhoneNumber? phone, out string? error)
        {
            try
            {
                phone = Create(rawValue, countryCode);
                error = null;
                return true;
            }
            catch (ArgumentException ex)
            {
                phone = null;
                error = ex.Message;
                return false;
            }
        }

        private static string NormalizeToDigits(string input)
        {
            // Elimina todo salvo dígitos
            var chars = new List<char>(input.Length);
            foreach (var ch in input)
                if (char.IsDigit(ch)) chars.Add(ch);
            return new string(chars.ToArray());
        }

        private static string NormalizeCountryCode(string input)
        {
            var trimmed = input.Trim();
            if (!trimmed.StartsWith("+"))
                trimmed = "+" + NormalizeToDigits(trimmed);
            return trimmed;
        }

        public static PhoneNumber FromE164(string e164)
        {
            if (string.IsNullOrWhiteSpace(e164))
                throw new ArgumentException("Invalid E.164 phone number", nameof(e164));

            // intenta 1, 2 y 3 dígitos de country code y valida contra Patterns
            if (e164[0] != '+') throw new ArgumentException("E.164 must start with '+'", nameof(e164));

            // probamos longitudes 1..3
            for (int len = 1; len <= 3; len++)
            {
                if (e164.Length <= 1 + len) break;
                var cc = e164.Substring(0, 1 + len);          // "+57", "+1", "+351", etc.
                var rest = e164.Substring(1 + len);           // el nacional
                var national = NormalizeToDigits(rest);

                if (Patterns.TryGetValue(cc, out var regex) && regex.IsMatch(national))
                {
                    var normalized = cc + national;
                    return new PhoneNumber(normalized, cc, national);
                }
            }

            throw new ArgumentException($"Unsupported or invalid E.164 number '{e164}'", nameof(e164));
        }

        public override string ToString() => E164;
    }
}
