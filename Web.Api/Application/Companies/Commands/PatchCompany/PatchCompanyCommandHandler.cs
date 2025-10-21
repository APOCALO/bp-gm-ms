using AutoMapper;
using ErrorOr;
using Framework.Application.Common;
using Framework.Application.Interfaces;
using Framework.Domain.Primitives;
using Framework.Domain.ValueObjects;
using Web.Api.Application.Companies.DTOs;
using Web.Api.Application.Interfaces.Repositories;

namespace Web.Api.Application.Companies.Commands.PatchCompany
{
    internal sealed class PatchCompanyCommandHandler : ApiBaseHandler<PatchCompanyCommand, CompanyResponseDTO>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICompanyRepository _companyRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly IRedisCacheService _cache;

        private const string BUCKETNAME = "companies-photos";
        private const int URL_EXPIRY_SECONDS = 3600;

        public PatchCompanyCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<PatchCompanyCommandHandler> logger,
            ICompanyRepository companyRepository,
            IFileStorageService fileStorageService,
            IRedisCacheService cache)
            : base(logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _companyRepository = companyRepository ?? throw new ArgumentNullException(nameof(companyRepository));
            _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        protected override async Task<ErrorOr<ApiResponse<CompanyResponseDTO>>> HandleRequest(
            PatchCompanyCommand request,
            CancellationToken cancellationToken)
        {
            var companyId = request.Id;
            var existingCompany = await _companyRepository.GetByIdAsync(companyId, cancellationToken);

            if (existingCompany is null)
            {
                return Error.NotFound("UpdateCompany.NotFound", $"Company with ID {request.Id} not found.");
            }

            var phoneNumberResult = ValidatePhoneNumber(request.PhoneNumber, request.CountryCode);
            if (phoneNumberResult.IsError)
            {
                return phoneNumberResult.Errors;
            }

            var addressResult = ValidateAddress(request, existingCompany.Address);
            if (addressResult.IsError)
            {
                return addressResult.Errors;
            }

            var scheduleResult = ValidateSchedule(request, existingCompany.Schedule);
            if (scheduleResult.IsError)
            {
                return scheduleResult.Errors;
            }

            if (request.Name is { } name)
            {
                existingCompany.UpdateName(name);
            }

            if (request.Slogan is { } slogan)
            {
                existingCompany.UpdateSlogan(slogan);
            }

            if (request.Description is { } description)
            {
                existingCompany.UpdateDescription(description);
            }

            if (request.Website is { } website)
            {
                existingCompany.UpdateWebsite(website);
            }

            if (addressResult.Value is Address newAddress)
            {
                existingCompany.UpdateAddress(newAddress);
            }

            if (phoneNumberResult.Value is PhoneNumber newPhone)
            {
                existingCompany.UpdatePhoneNumber(newPhone);
            }

            if (scheduleResult.Value is WorkSchedule newSchedule)
            {
                existingCompany.UpdateSchedule(newSchedule);
            }

            if (request.WorksOnHolidays.HasValue)
            {
                existingCompany.SetWorksOnHolidays(request.WorksOnHolidays.Value);
            }

            if (request.FlexibleHours.HasValue)
            {
                existingCompany.SetFlexibleHours(request.FlexibleHours.Value);
            }

            if (request.TimeZone.HasValue)
            {
                existingCompany.UpdateTimeZone(request.TimeZone.Value);
            }

            if (request.IsActive.HasValue)
            {
                if (request.IsActive.Value)
                {
                    existingCompany.Activate();
                }
                else
                {
                    existingCompany.Deactivate();
                }
            }

            if (request.NewCompanyPhotos?.Any() == true)
            {
                var oldPhotoKeys = existingCompany.CompanyPhotos?.ToList() ?? new List<string>();
                var newPhotoKeys = await UploadPhotosAsync(companyId, request.NewCompanyPhotos);
                existingCompany.SetPhotos(newPhotoKeys);

                await DeletePhotosAsync(companyId, oldPhotoKeys.Except(newPhotoKeys));
            }

            existingCompany.SetAuditUpdate(request.UpdatedById);

            _companyRepository.Update(existingCompany);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (existingCompany.CompanyPhotos is { Count: > 0 })
            {
                var signedUrls = await GenerateAndCacheSignedUrlsAsync(companyId, existingCompany.CompanyPhotos);
                existingCompany.SetCoverPhotoUrls(signedUrls);
            }
            else
            {
                await ClearCachedPhotosAsync(companyId);
                existingCompany.SetCoverPhotoUrls(Array.Empty<string>());
            }

            var mapped = _mapper.Map<CompanyResponseDTO>(existingCompany);
            return new ApiResponse<CompanyResponseDTO>(mapped, true);
        }

        #region Helpers

        private static ErrorOr<PhoneNumber?> ValidatePhoneNumber(string? phone, string? countryCode)
        {
            var hasPhone = !string.IsNullOrWhiteSpace(phone);
            var hasCountry = !string.IsNullOrWhiteSpace(countryCode);

            if (!hasPhone && !hasCountry)
            {
                return (PhoneNumber?)null;
            }

            if (hasPhone ^ hasCountry)
            {
                return Error.Validation("PatchCompany.PhoneNumber", "Phone number and country code must be provided together.");
            }

            try
            {
                return PhoneNumber.Create(phone!, countryCode!);
            }
            catch (ArgumentException ex)
            {
                return Error.Validation("PatchCompany.PhoneNumber", ex.Message);
            }
        }

        private static ErrorOr<Address?> ValidateAddress(PatchCompanyCommand request, Address currentAddress)
        {
            if (request.Country is null &&
                request.Department is null &&
                request.City is null &&
                request.StreetType is null &&
                request.StreetNumber is null &&
                request.CrossStreetNumber is null &&
                request.PropertyNumber is null &&
                request.ZipCode is null)
            {
                return (Address?)null;
            }

            var country = request.Country ?? currentAddress.Country;
            var department = request.Department ?? currentAddress.Department;
            var city = request.City ?? currentAddress.City;
            var streetType = request.StreetType ?? currentAddress.StreetType;
            var streetNumber = request.StreetNumber ?? currentAddress.StreetNumber;
            var crossStreetNumber = request.CrossStreetNumber ?? currentAddress.CrossStreetNumber;
            var propertyNumber = request.PropertyNumber ?? currentAddress.PropertyNumber;
            var zipCode = request.ZipCode ?? currentAddress.ZipCode;

            try
            {
                return Address.Create(country, department, city, streetType, streetNumber, crossStreetNumber, propertyNumber, zipCode);
            }
            catch (Exception ex) when (ex is ArgumentException or ArgumentNullException)
            {
                return Error.Validation("PatchCompany.Address", ex.Message);
            }
        }

        private static ErrorOr<WorkSchedule?> ValidateSchedule(PatchCompanyCommand request, WorkSchedule currentSchedule)
        {
            var hasAnyField =
                request.WorkingDays is not null ||
                request.OpeningHour.HasValue ||
                request.ClosingHour.HasValue ||
                request.LunchStart.HasValue ||
                request.LunchEnd.HasValue ||
                request.AllowAppointmentsDuringLunch.HasValue ||
                request.AppointmentDurationMinutes.HasValue;

            if (!hasAnyField)
            {
                return (WorkSchedule?)null;
            }

            var workingDays = request.WorkingDays ?? currentSchedule.WorkingDays;
            var openingHour = request.OpeningHour ?? currentSchedule.OpeningHour;
            var closingHour = request.ClosingHour ?? currentSchedule.ClosingHour;
            var lunchStart = request.LunchStart ?? currentSchedule.LunchStart;
            var lunchEnd = request.LunchEnd ?? currentSchedule.LunchEnd;
            var allowDuringLunch = request.AllowAppointmentsDuringLunch ?? currentSchedule.AllowAppointmentsDuringLunch;
            var appointmentDuration = request.AppointmentDurationMinutes ?? currentSchedule.AppointmentDurationMinutes;

            try
            {
                return WorkSchedule.Create(
                    workingDays,
                    openingHour,
                    closingHour,
                    lunchStart,
                    lunchEnd,
                    allowDuringLunch,
                    appointmentDuration);
            }
            catch (Exception ex) when (ex is ArgumentException or ArgumentNullException)
            {
                return Error.Validation("PatchCompany.Schedule", ex.Message);
            }
        }

        private async Task DeletePhotosAsync(Guid companyId, IEnumerable<string> photoKeys)
        {
            var deleteTasks = photoKeys.Select(async key =>
            {
                var deleteResult = await _fileStorageService.DeleteFileAsync(BUCKETNAME, key);

                if (deleteResult.IsError)
                {
                    _logger.LogWarning("Failed to delete photo {PhotoKey} for company {CompanyId}. Error: {Error}",
                        key, companyId, deleteResult.FirstError.Description);
                }
            });

            await Task.WhenAll(deleteTasks);
        }

        private async Task ClearCachedPhotosAsync(Guid companyId)
        {
            var cacheKey = $"v1:company:{companyId}:photos";
            await _cache.RemoveAsync(cacheKey);
        }

        private async Task<List<string>> UploadPhotosAsync(Guid companyId, IEnumerable<IFormFile> photos)
        {
            var uploadTasks = photos.Select(async (photo, index) =>
            {
                var extension = Path.GetExtension(photo.FileName);
                var objectName = $"{companyId}/photo_{index + 1}{extension}";
                var contentType = photo.ContentType ?? "image/png";

                await using var stream = photo.OpenReadStream();
                var uploadResult = await _fileStorageService.UploadFileAsync(BUCKETNAME, objectName, stream, contentType);

                return uploadResult.IsError ? null : objectName;
            });

            var uploaded = await Task.WhenAll(uploadTasks);
            return uploaded.Where(x => x != null).Cast<string>().ToList();
        }

        private async Task<List<string>> GenerateAndCacheSignedUrlsAsync(Guid companyId, IReadOnlyCollection<string> photoKeys)
        {
            var signedUrlTasks = photoKeys.Select(async key =>
            {
                var urlResult = await _fileStorageService.GetFileUrlAsync(BUCKETNAME, key, URL_EXPIRY_SECONDS);
                return urlResult.IsError ? null : urlResult.Value;
            });

            var signedUrls = (await Task.WhenAll(signedUrlTasks))
                .Where(url => url != null)
                .Cast<string>()
                .ToList();

            if (signedUrls.Count > 0)
            {
                var cacheKey = $"v1:company:{companyId}:photos";
                var cacheTtl = TimeSpan.FromSeconds(URL_EXPIRY_SECONDS - 60);
                await _cache.SetAsync(cacheKey, signedUrls, cacheTtl);
            }

            return signedUrls;
        }

        #endregion
    }
}
