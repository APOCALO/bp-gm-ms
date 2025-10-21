using Framework.Application.Common;
using Web.Api.Application.Companies.DTOs;
using Web.Api.Domain.Companies;

namespace Web.Api.Application.Companies.Commands.CreateCompany
{
    public record CreateCompanyCommand : BaseResponse<CompanyResponseDTO>
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string Name { get; init; }
        public string? Description { get; init; }
        public string Slogan { get; set; }
        public List<IFormFile> CompanyPhotos { get; init; } = new();
        public string PhoneNumber { get; init; }
        public string CountryCode { get; init; }
        public string Country { get; init; }
        public string Department { get; init; }
        public string City { get; init; }
        public string StreetType { get; init; }
        public string StreetNumber { get; init; }
        public string? CrossStreetNumber { get; init; }
        public string PropertyNumber { get; init; }
        public string? ZipCode { get; init; }
        public string? Website { get; init; }
        public required IReadOnlyList<DayOfWeek> WorkingDays { get; init; }
        public TimeSpan OpeningHour { get; init; }
        public TimeSpan ClosingHour { get; init; }
        public TimeSpan? LunchStart { get; init; }
        public TimeSpan? LunchEnd { get; init; }
        public bool AllowAppointmentsDuringLunch { get; init; }
        public int AppointmentDurationMinutes { get; init; }
        public bool WorksOnHolidays { get; init; }
        public bool FlexibleHours { get; init; }
        public TimeZoneEnum TimeZone { get; init; }
        public bool IsActive { get; init; } = true;
        public Guid CreatedById { get; init; }
    }
}
