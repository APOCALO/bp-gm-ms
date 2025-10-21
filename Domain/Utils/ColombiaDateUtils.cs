namespace Domain.Utils
{
    public static class ColombiaDateUtils
    {
        private const string COLOMBIA_TIME = "SA Pacific Standard Time";

        /// <summary>
        /// Realiza el ajuste entre la hora de Azure (UTC) y la hora de Colombia en una fecha.
        /// </summary>
        /// <param name="date">Fecha/hora en UTC</param>
        /// <returns>Fecha/hora convertida a Colombia</returns>
        public static DateTime ToColombiaTime(this DateTime date)
        {
            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(COLOMBIA_TIME);
            return TimeZoneInfo.ConvertTimeFromUtc(date.ToUniversalTime(), timeZone);
        }

        /// <summary>
        /// Por defecto es DateTime.UtcNow, pero puede sobreescribirse con SetDateTime(...) para pruebas.
        /// </summary>
        public static Func<DateTime> Now = () => DateTime.UtcNow;

        /// <summary>
        /// Setea una fecha fija para pruebas.
        /// </summary>
        public static void SetDateTime(DateTime dateTimeNowUtc)
        {
            Now = () => dateTimeNowUtc;
        }

        /// <summary>
        /// Restaura el comportamiento normal para que Now devuelva DateTime.UtcNow.
        /// </summary>
        public static void ResetDateTime()
        {
            Now = () => DateTime.UtcNow;
        }

        /// <summary>
        /// Devuelve el primer segundo del mes actual en UTC, tomando como referencia la hora local de Colombia.
        /// </summary>
        public static DateTime GetFirstDateOfMonth()
        {
            // Hora actual de Colombia
            var localDate = Now().ToColombiaTime();

            // Primer día del mes en hora local de Colombia
            var firstLocal = new DateTime(localDate.Year, localDate.Month, 1, 0, 0, 0, DateTimeKind.Unspecified);

            // Convertirlo a UTC para guardar en PostgreSQL
            var colombiaTz = TimeZoneInfo.FindSystemTimeZoneById(COLOMBIA_TIME);
            return TimeZoneInfo.ConvertTimeToUtc(firstLocal, colombiaTz);
        }

        /// <summary>
        /// Devuelve el último segundo del mes actual en UTC, tomando como referencia la hora local de Colombia.
        /// </summary>
        public static DateTime GetLastDateOfMonth()
        {
            return GetFirstDateOfMonth().AddMonths(1).AddTicks(-1);
        }

        /// <summary>
        /// Devuelve el primer día de la semana actual (lunes) en UTC, tomando como referencia la hora local de Colombia.
        /// </summary>
        public static DateTime GetFirstDayOfWeek()
        {
            var localDate = Now().ToColombiaTime();

            // Retroceder hasta el lunes
            while (localDate.DayOfWeek != DayOfWeek.Monday)
            {
                localDate = localDate.AddDays(-1);
            }

            var firstLocal = new DateTime(localDate.Year, localDate.Month, localDate.Day, 0, 0, 0, DateTimeKind.Unspecified);
            var colombiaTz = TimeZoneInfo.FindSystemTimeZoneById(COLOMBIA_TIME);
            return TimeZoneInfo.ConvertTimeToUtc(firstLocal, colombiaTz);
        }

        /// <summary>
        /// Devuelve el último día de la semana actual (domingo) en UTC, tomando como referencia la hora local de Colombia.
        /// </summary>
        public static DateTime GetLastDayOfWeek()
        {
            return GetFirstDayOfWeek().AddDays(7).AddTicks(-1);
        }
    }
}
