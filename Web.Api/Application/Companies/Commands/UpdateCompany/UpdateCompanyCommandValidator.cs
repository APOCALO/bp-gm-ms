using FluentValidation;

namespace Web.Api.Application.Companies.Commands.UpdateCompany
{
    public class UpdateCompanyCommandValidator : AbstractValidator<UpdateCompanyCommand>
    {
        public UpdateCompanyCommandValidator()
        {
            // ✅ Nombre obligatorio, máximo 100 caracteres (igual que EF)
            RuleFor(c => c.Name)
                .NotEmpty().WithMessage("El nombre de la empresa es obligatorio.")
                .MaximumLength(100).WithMessage("El nombre no puede superar los 100 caracteres.");

            // ✅ Descripción opcional pero hasta 500 (igual que EF)
            RuleFor(c => c.Description)
                .MaximumLength(500).WithMessage("La descripción no puede superar los 500 caracteres.");

            // ✅ Slogan obligatorio, máximo 250 caracteres (igual que EF)
            RuleFor(c => c.Slogan)
                .NotEmpty().WithMessage("El slogan de la empresa es obligatorio.")
                .MaximumLength(250).WithMessage("El slogan no puede superar los 250 caracteres.");

            // ✅ Teléfono obligatorio con máximo 15 dígitos (igual que EF)
            RuleFor(c => c.PhoneNumber)
                .NotEmpty().WithMessage("El número de teléfono es obligatorio.")
                .MaximumLength(15).WithMessage("El número de teléfono no puede superar los 15 caracteres.");

            RuleFor(c => c.CountryCode)
                .NotEmpty().WithMessage("El código de país es obligatorio.")
                .MaximumLength(4).WithMessage("El código de país no puede superar los 4 caracteres.");

            // ✅ Dirección obligatoria
            RuleFor(c => c.Country)
                .NotEmpty().WithMessage("El país es obligatorio.")
                .MaximumLength(100);

            RuleFor(c => c.Department)
                .NotEmpty().WithMessage("El departamento es obligatorio.")
                .MaximumLength(100);

            RuleFor(c => c.City)
                .NotEmpty().WithMessage("La ciudad es obligatoria.")
                .MaximumLength(100);

            RuleFor(c => c.StreetType)
                .NotEmpty().WithMessage("El tipo de calle es obligatorio.")
                .MaximumLength(20);

            RuleFor(c => c.StreetNumber)
                .NotEmpty().WithMessage("El número de la calle es obligatorio.")
                .MaximumLength(20);

            RuleFor(c => c.CrossStreetNumber)
                .MaximumLength(20);

            RuleFor(c => c.PropertyNumber)
                .NotEmpty().WithMessage("El número del inmueble es obligatorio.")
                .MaximumLength(20);

            RuleFor(c => c.ZipCode)
                .MaximumLength(20);

            // ✅ Sitio web opcional pero máximo 200 (igual que EF)
            RuleFor(c => c.Website)
                .MaximumLength(200);

            // ✅ Horarios de trabajo obligatorios
            RuleFor(c => c.WorkingDays)
                .NotEmpty().WithMessage("Debe especificar al menos un día hábil.");

            RuleFor(c => c.OpeningHour)
                .NotEmpty().WithMessage("La hora de apertura es obligatoria.");

            RuleFor(c => c.ClosingHour)
                .NotEmpty().WithMessage("La hora de cierre es obligatoria.")
                .GreaterThan(c => c.OpeningHour)
                .WithMessage("La hora de cierre debe ser posterior a la hora de apertura.");

            // ✅ Duración mínima de cita
            RuleFor(c => c.AppointmentDurationMinutes)
                .GreaterThan(0).WithMessage("La duración de las citas debe ser mayor a 0 minutos.");

            RuleFor(b => b.TimeZone)
                .IsInEnum()
                .WithMessage("La zona horaria seleccionada no es válida.");

            // ✅ Validar coherencia si hay almuerzo
            When(c => c.LunchStart.HasValue || c.LunchEnd.HasValue, () =>
            {
                RuleFor(c => c.LunchStart)
                    .NotNull().WithMessage("Debe especificar la hora de inicio del almuerzo si se define hora de fin.");

                RuleFor(c => c.LunchEnd)
                    .NotNull().WithMessage("Debe especificar la hora de fin del almuerzo si se define hora de inicio.")
                    .GreaterThan(c => c.LunchStart.GetValueOrDefault())
                    .WithMessage("La hora de fin del almuerzo debe ser posterior a la hora de inicio.");
            });

            // ✅ Validar que el booleano sea explícito
            RuleFor(c => c.WorksOnHolidays)
                .NotNull().WithMessage("Debe especificar si trabaja en festivos.");

            RuleFor(c => c.FlexibleHours)
                .NotNull().WithMessage("Debe especificar si tiene horarios flexibles.");

            RuleFor(c => c.UpdatedById)
                .NotEqual(Guid.Empty).WithMessage("El Id del usuario no puede ser Guid.Empty.");
        }
    }
}
