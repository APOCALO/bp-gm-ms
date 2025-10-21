using Framework.Domain.ValueObjects;
using Web.Api.Domain.Common;

namespace Web.Api.Application.Companies.DTOs
{
    public record CompanyResponseDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Slogan { get; set; }
        public string? Description { get; set; }
        public List<string> CoverPhotoUrls { get; set; } = new();
        public List<string> CompanyPhotoIds { get; set; } = new();
        public Address Address { get; set; }
        public PhoneNumber PhoneNumber { get; set; }
        public string? Website { get; set; }
        public WorkSchedule Schedule { get; set; }
        public bool WorksOnHolidays { get; set; }
        public bool FlexibleHours { get; set; }
        public TimeZoneEnum TimeZone { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedById { get; set; }
    }
}
