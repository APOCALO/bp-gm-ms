namespace Domain.ValueObjects
{
    public sealed record Address
    {
        public string Country { get; init; }
        public string Department { get; init; }
        public string City { get; init; }
        public string StreetType { get; init; }
        public string StreetNumber { get; init; }
        public string? CrossStreetNumber { get; init; }
        public string PropertyNumber { get; init; }
        public string? ZipCode { get; init; }

        private static readonly HashSet<string> StreetTypesCOL = new(StringComparer.OrdinalIgnoreCase)
        {
            "carrera", "calle", "diagonal", "avenida"
        };

        // Constructor privado para EF Core
        private Address() { }

        private Address(
            string country,
            string department,
            string city,
            string streetType,
            string streetNumber,
            string? crossStreetNumber,
            string propertyNumber,
            string? zipCode)
        {
            Country = country;
            Department = department;
            City = city;
            StreetType = streetType;
            StreetNumber = streetNumber;
            CrossStreetNumber = crossStreetNumber;
            PropertyNumber = propertyNumber;
            ZipCode = zipCode;
        }

        public static Address Create(
            string country,
            string department,
            string city,
            string streetType,
            string streetNumber,
            string? crossStreetNumber,
            string propertyNumber,
            string? zipCode)
        {
            if (string.IsNullOrWhiteSpace(country))
                throw new ArgumentException("Country is required", nameof(country));

            if (string.IsNullOrWhiteSpace(department))
                throw new ArgumentException("Department is required", nameof(department));

            if (string.IsNullOrWhiteSpace(city))
                throw new ArgumentException("City is required", nameof(city));

            if (string.IsNullOrWhiteSpace(streetType))
                throw new ArgumentException("StreetType is required", nameof(streetType));

            if (string.IsNullOrWhiteSpace(streetNumber))
                throw new ArgumentException("StreetNumber is required", nameof(streetNumber));

            if (string.IsNullOrWhiteSpace(propertyNumber))
                throw new ArgumentException("PropertyNumber is required", nameof(propertyNumber));

            // Validación condicional por país
            if (country.Equals("Colombia", StringComparison.OrdinalIgnoreCase) &&
                !StreetTypesCOL.Contains(streetType))
            {
                throw new ArgumentException($"StreetType '{streetType}' is invalid for Colombia", nameof(streetType));
            }

            return new Address(
                country.Trim(),
                department.Trim(),
                city.Trim(),
                streetType.Trim(),
                streetNumber.Trim(),
                string.IsNullOrWhiteSpace(crossStreetNumber) ? null : crossStreetNumber.Trim(),
                propertyNumber.Trim(),
                string.IsNullOrWhiteSpace(zipCode) ? null : zipCode.Trim()
            );
        }

        public override string ToString()
        {
            var crossPart = string.IsNullOrWhiteSpace(CrossStreetNumber)
                ? string.Empty
                : $"{CrossStreetNumber}-";

            var zipPart = string.IsNullOrWhiteSpace(ZipCode)
                ? string.Empty
                : $", {ZipCode}";

            return $"{StreetType} {StreetNumber} #{crossPart}{PropertyNumber}, {City}, {Department}, {Country}{zipPart}";
        }
    }
}
