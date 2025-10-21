using Framework.Application.Common;
using Web.Api.Application.Companies.DTOs;
using Web.Api.Domain.Common;

namespace Web.Api.Application.Companies.Commands.PatchCompany
{
    public record PatchCompanyCommand : BaseResponse<CompanyResponseDTO>
    {
        public Guid Id { get; init; }
        public string? Name { get; init; }
        public string? Slogan { get; init; }
        public string? Description { get; init; }
        public List<IFormFile>? NewCompanyPhotos { get; init; }
        public string? PhoneNumber { get; init; }
        public string? CountryCode { get; init; }
        public string? Country { get; init; }
        public string? Department { get; init; }
        public string? City { get; init; }
        public string? StreetType { get; init; }
        public string? StreetNumber { get; init; }
        public string? CrossStreetNumber { get; init; }
        public string? PropertyNumber { get; init; }
        public string? ZipCode { get; init; }
        public string? Website { get; init; }
        public IReadOnlyList<DayOfWeek>? WorkingDays { get; init; }
        public TimeSpan? OpeningHour { get; init; } = null;
        public TimeSpan? ClosingHour { get; init; } = null;
        public TimeSpan? LunchStart { get; init; } = null;
        public TimeSpan? LunchEnd { get; init; } = null;
        public bool? AllowAppointmentsDuringLunch { get; init; }
        public int? AppointmentDurationMinutes { get; init; }
        public bool? WorksOnHolidays { get; init; }
        public bool? FlexibleHours { get; init; }
        public TimeZoneEnum? TimeZone { get; init; }
        public bool? IsActive { get; init; }
        public Guid UpdatedById { get; init; }
    }
}
