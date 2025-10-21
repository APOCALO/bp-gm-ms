using Framework.Domain.Primitives;
using Framework.Domain.ValueObjects;
using Web.Api.Domain.Common;

namespace Web.Api.Domain.Companies
{
    public sealed class Company : AggregateRoot
    {
        public string Name { get; private set; }
        public string Slogan { get; private set; }
        public string? Description { get; private set; }
        public List<string> CoverPhotoUrls { get; private set; } = new();
        public List<string> CompanyPhotos { get; private set; } = new();
        public Address Address { get; private set; }
        public PhoneNumber PhoneNumber { get; private set; }
        public string? Website { get; private set; }
        public WorkSchedule Schedule { get; private set; }
        public bool WorksOnHolidays { get; private set; }
        public bool FlexibleHours { get; private set; }
        public TimeZoneEnum TimeZone { get; private set; } = TimeZoneEnum.UTC;
        public bool IsActive { get; private set; } = true;

        private Company() { }

        private Company(
            Guid? id,
            Guid createdById,
            string name,
            string slogan,
            string? description,
            List<string> coverPhotoUrls,
            List<string> companyPhotos,
            Address address,
            PhoneNumber phoneNumber,
            string? website,
            WorkSchedule schedule,
            bool worksOnHolidays,
            bool flexibleHours,
            TimeZoneEnum timeZone,
            bool isActive) : base(createdById, id)
        {
            Name = name;
            Slogan = slogan;
            Description = description;
            CoverPhotoUrls = coverPhotoUrls;
            CompanyPhotos = companyPhotos;
            Address = address;
            PhoneNumber = phoneNumber;
            Website = website;
            Schedule = schedule;
            WorksOnHolidays = worksOnHolidays;
            FlexibleHours = flexibleHours;
            TimeZone = timeZone;
            IsActive = isActive;
        }

        public static Company Create(
            Guid createdById,
            string name,
            string slogan,
            string? description,
            IEnumerable<string>? coverPhotoUrls,
            IEnumerable<string>? companyPhotos,
            Address address,
            PhoneNumber phoneNumber,
            string? website,
            WorkSchedule schedule,
            bool worksOnHolidays,
            bool flexibleHours,
            TimeZoneEnum timeZone,
            bool isActive = true,
            Guid? id = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Company name cannot be empty.", nameof(name));

            if (address is null)
                throw new ArgumentNullException(nameof(address));

            if (phoneNumber is null)
                throw new ArgumentNullException(nameof(phoneNumber));

            if (schedule is null)
                throw new ArgumentNullException(nameof(schedule));

            var coverPhotos = coverPhotoUrls?.Where(url => !string.IsNullOrWhiteSpace(url)).Distinct().ToList()
                ?? new List<string>();

            var storedPhotos = companyPhotos?.Where(photo => !string.IsNullOrWhiteSpace(photo)).Distinct().ToList()
                ?? new List<string>();

            return new Company(
                id,
                createdById,
                name.Trim(),
                slogan,
                description,
                coverPhotos,
                storedPhotos,
                address,
                phoneNumber,
                website,
                schedule,
                worksOnHolidays,
                flexibleHours,
                timeZone,
                isActive);
        }

        #region Domain Methods

        public void UpdateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Company name cannot be empty.", nameof(name));

            Name = name.Trim();
        }

        public void UpdateSlogan(string slogan)
        {
            if (string.IsNullOrWhiteSpace(slogan))
                throw new ArgumentException("Slogan cannot be empty.", nameof(slogan));

            Slogan = slogan.Trim();
        }

        public void UpdateDescription(string? description) => Description = description;
        public void UpdateWebsite(string? website) => Website = website;
        public void UpdateAddress(Address address) => Address = address ?? throw new ArgumentNullException(nameof(address));
        public void UpdatePhoneNumber(PhoneNumber phoneNumber) => PhoneNumber = phoneNumber ?? throw new ArgumentNullException(nameof(phoneNumber));
        public void UpdateSchedule(WorkSchedule schedule) => Schedule = schedule ?? throw new ArgumentNullException(nameof(schedule));
        public void SetWorksOnHolidays(bool worksOnHolidays) => WorksOnHolidays = worksOnHolidays;
        public void SetFlexibleHours(bool flexibleHours) => FlexibleHours = flexibleHours;
        public void UpdateTimeZone(TimeZoneEnum timeZone) => TimeZone = timeZone;
        public void Deactivate() => IsActive = false;
        public void Activate() => IsActive = true;

        public void SetPhotos(IEnumerable<string> photos)
        {
            if (photos is null)
                throw new ArgumentNullException(nameof(photos));

            CompanyPhotos = photos
                .Where(photo => !string.IsNullOrWhiteSpace(photo))
                .Distinct()
                .ToList();
        }

        public void SetCoverPhotoUrls(IEnumerable<string> coverPhotoUrls)
        {
            if (coverPhotoUrls is null)
                throw new ArgumentNullException(nameof(coverPhotoUrls));

            CoverPhotoUrls = coverPhotoUrls
                .Where(url => !string.IsNullOrWhiteSpace(url))
                .Distinct()
                .ToList();
        }

        #endregion
    }
}
