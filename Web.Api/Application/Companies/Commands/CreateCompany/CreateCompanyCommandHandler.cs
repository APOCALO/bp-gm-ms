using AutoMapper;
using ErrorOr;
using Framework.Application.Common;
using Framework.Application.Interfaces;
using Framework.Domain.Primitives;
using Framework.Domain.ValueObjects;
using Web.Api.Application.Companies.DTOs;
using Web.Api.Application.Interfaces.Repositories;
using Web.Api.Domain.Companies;

namespace Web.Api.Application.Companies.Commands.CreateCompany
{
    internal sealed class CreateCompanyCommandHandler : ApiBaseHandler<CreateCompanyCommand, CompanyResponseDTO>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICompanyRepository _companyRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly IRedisCacheService _cache;

        private const string BUCKETNAME = "companies-photos";
        private const int URL_EXPIRY_SECONDS = 3600;

        public CreateCompanyCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<CreateCompanyCommandHandler> logger,
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
            CreateCompanyCommand request,
            CancellationToken cancellationToken)
        {
            var phoneNumberResult = ValidatePhoneNumber(request.PhoneNumber, request.CountryCode);
            if (phoneNumberResult.IsError)
            {
                return phoneNumberResult.Errors;
            }

            var addressResult = ValidateAddress(request);
            if (addressResult.IsError)
            {
                return addressResult.Errors;
            }

            var scheduleResult = ValidateSchedule(request);
            if (scheduleResult.IsError)
            {
                return scheduleResult.Errors;
            }

            var company = Company.Create(
                createdById: request.CreatedById,
                name: request.Name,
                slogan: request.Slogan,
                description: request.Description,
                coverPhotoUrls: Enumerable.Empty<string>(),
                companyPhotos: Enumerable.Empty<string>(),
                address: addressResult.Value,
                phoneNumber: phoneNumberResult.Value,
                website: request.Website,
                schedule: scheduleResult.Value,
                worksOnHolidays: request.WorksOnHolidays,
                flexibleHours: request.FlexibleHours,
                timeZone: request.TimeZone,
                isActive: request.IsActive,
                id: request.Id == Guid.Empty ? null : request.Id);

            if (request.CompanyPhotos?.Any() == true)
            {
                var successfulPhotos = await UploadPhotosAsync(company.Id, request.CompanyPhotos);
                company.SetPhotos(successfulPhotos);
            }

            await _companyRepository.AddAsync(company, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (company.CompanyPhotos is not { Count: > 0 })
            {
                return new ApiResponse<CompanyResponseDTO>(_mapper.Map<CompanyResponseDTO>(company), true);
            }

            var signedUrls = await GenerateAndCacheSignedUrlsAsync(company.Id, company.CompanyPhotos);
            company.SetCoverPhotoUrls(signedUrls);

            var mappedResult = _mapper.Map<CompanyResponseDTO>(company);
            return new ApiResponse<CompanyResponseDTO>(mappedResult, true);
        }

        #region Helpers

        private static ErrorOr<PhoneNumber> ValidatePhoneNumber(string phone, string countryCode)
        {
            try
            {
                return PhoneNumber.Create(phone, countryCode);
            }
            catch (ArgumentException ex)
            {
                return Error.Validation("CreateCompany.PhoneNumber", ex.Message);
            }
        }

        private static ErrorOr<Address> ValidateAddress(CreateCompanyCommand request)
        {
            try
            {
                return Address.Create(
                    request.Country,
                    request.Department,
                    request.City,
                    request.StreetType,
                    request.StreetNumber,
                    request.CrossStreetNumber,
                    request.PropertyNumber,
                    request.ZipCode);
            }
            catch (Exception ex) when (ex is ArgumentException or ArgumentNullException)
            {
                return Error.Validation("CreateCompany.Address", ex.Message);
            }
        }

        private static ErrorOr<WorkSchedule> ValidateSchedule(CreateCompanyCommand request)
        {
            try
            {
                return WorkSchedule.Create(
                    request.WorkingDays,
                    request.OpeningHour,
                    request.ClosingHour,
                    request.LunchStart,
                    request.LunchEnd,
                    request.AllowAppointmentsDuringLunch,
                    request.AppointmentDurationMinutes);
            }
            catch (Exception ex) when (ex is ArgumentException or ArgumentNullException)
            {
                return Error.Validation("CreateCompany.Schedule", ex.Message);
            }
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
