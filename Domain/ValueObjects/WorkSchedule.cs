namespace Domain.ValueObjects
{
    public sealed record WorkSchedule
    {
        public IReadOnlyList<DayOfWeek> WorkingDays { get; }
        public TimeSpan OpeningHour { get; }
        public TimeSpan ClosingHour { get; }
        public TimeSpan? LunchStart { get; }
        public TimeSpan? LunchEnd { get; }
        public bool AllowAppointmentsDuringLunch { get; }
        public int AppointmentDurationMinutes { get; }

        // Constructor requerido por EF Core
        private WorkSchedule() { }

        private WorkSchedule(
            IEnumerable<DayOfWeek> workingDays,
            TimeSpan openingHour,
            TimeSpan closingHour,
            TimeSpan? lunchStart,
            TimeSpan? lunchEnd,
            bool allowAppointmentsDuringLunch,
            int appointmentDurationMinutes)
        {
            var days = workingDays?.Distinct().ToList() ?? throw new ArgumentNullException(nameof(workingDays));
            if (!days.Any())
                throw new ArgumentException("At least one working day must be specified.", nameof(workingDays));

            if (openingHour >= closingHour)
                throw new ArgumentException("Opening hour must be before closing hour.");

            if (appointmentDurationMinutes <= 0)
                throw new ArgumentException("Appointment duration must be greater than zero.");

            // Validación coherente de almuerzo
            if (lunchStart.HasValue && lunchEnd.HasValue)
            {
                if (lunchStart >= lunchEnd)
                    throw new ArgumentException("Lunch start must be before lunch end.");
                if (lunchStart < openingHour || lunchStart > closingHour)
                    throw new ArgumentException("Lunch start must be within working hours.");
                if (lunchEnd < openingHour || lunchEnd > closingHour)
                    throw new ArgumentException("Lunch end must be within working hours.");
            }

            WorkingDays = days.AsReadOnly();
            OpeningHour = openingHour;
            ClosingHour = closingHour;
            LunchStart = lunchStart;
            LunchEnd = lunchEnd;
            AllowAppointmentsDuringLunch = allowAppointmentsDuringLunch;
            AppointmentDurationMinutes = appointmentDurationMinutes;
        }

        /// <summary>
        /// Método de factoría estático para crear un horario de trabajo
        /// </summary>
        public static WorkSchedule Create(
            IEnumerable<DayOfWeek> workingDays,
            TimeSpan openingHour,
            TimeSpan closingHour,
            TimeSpan? lunchStart,
            TimeSpan? lunchEnd,
            bool allowAppointmentsDuringLunch,
            int appointmentDurationMinutes)
        {
            if (workingDays == null || !workingDays.Any())
                throw new ArgumentException("You must specify at least one business day.", nameof(workingDays));

            return new WorkSchedule(
                workingDays,
                openingHour,
                closingHour,
                lunchStart,
                lunchEnd,
                allowAppointmentsDuringLunch,
                appointmentDurationMinutes
            );
        }

        /// <summary>
        /// Tiempo total disponible para citas en minutos, excluyendo almuerzo si no se permiten citas en ese periodo
        /// </summary>
        private double EffectiveWorkingMinutes
        {
            get
            {
                double total = (ClosingHour - OpeningHour).TotalMinutes;

                if (!AllowAppointmentsDuringLunch && LunchStart.HasValue && LunchEnd.HasValue)
                {
                    total -= (LunchEnd.Value - LunchStart.Value).TotalMinutes;
                }

                return Math.Max(0, total);
            }
        }

        /// <summary>
        /// Cantidad máxima de citas posibles en un día considerando horario, almuerzo y duración de cada cita
        /// </summary>
        public int MaxAppointmentsPerDay =>
            (int)(EffectiveWorkingMinutes / AppointmentDurationMinutes);

        /// <summary>
        /// Verifica si una hora está dentro del horario laboral permitido
        /// </summary>
        public bool IsWithinWorkingHours(DayOfWeek day, TimeSpan time)
        {
            if (!WorkingDays.Contains(day)) return false;

            if (time < OpeningHour || time > ClosingHour) return false;

            if (!AllowAppointmentsDuringLunch && LunchStart.HasValue && LunchEnd.HasValue)
            {
                if (time >= LunchStart && time < LunchEnd)
                    return false;
            }

            return true;
        }
    }
}
