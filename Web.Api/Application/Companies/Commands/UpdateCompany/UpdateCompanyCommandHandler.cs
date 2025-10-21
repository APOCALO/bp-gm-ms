using AutoMapper;
using ErrorOr;
using Framework.Application.Common;
using Framework.Application.Interfaces;
using Framework.Domain.Primitives;
using Framework.Domain.ValueObjects;
using Web.Api.Application.Companies.DTOs;
using Web.Api.Application.Interfaces.Repositories;

namespace Web.Api.Application.Companies.Commands.UpdateCompany;

internal sealed class UpdateCompanyCommandHandler : ApiBaseHandler<UpdateCompanyCommand, CompanyResponseDTO>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICompanyRepository _companyRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IRedisCacheService _cache;

    private const string BUCKETNAME = "companies-photos";
    private const int URL_EXPIRY_SECONDS = 3600;

    public UpdateCompanyCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<UpdateCompanyCommandHandler> logger,
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
        UpdateCompanyCommand request,
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

        existingCompany.UpdateName(request.Name);
        existingCompany.UpdateSlogan(request.Slogan);
        existingCompany.UpdateDescription(request.Description);
        existingCompany.UpdateWebsite(request.Website);
        existingCompany.UpdateAddress(addressResult.Value);
        existingCompany.UpdatePhoneNumber(phoneNumberResult.Value);
        existingCompany.UpdateSchedule(scheduleResult.Value);
        existingCompany.SetWorksOnHolidays(request.WorksOnHolidays);
        existingCompany.SetFlexibleHours(request.FlexibleHours);
        existingCompany.UpdateTimeZone(request.TimeZone);

        if (request.IsActive)
        {
            existingCompany.Activate();
        }
        else
        {
            existingCompany.Deactivate();
        }

        var originalKeys = existingCompany.CompanyPhotos?.ToList() ?? new List<string>();

        var requestedDeletes = request.CompanyPhotosToDelete ?? new List<string>();

        var keptOldKeys = originalKeys.Where(k => !requestedDeletes.Contains(k)).ToList();

        var newKeys = request.NewCompanyPhotos?.Any() == true
            ? await UploadPhotosAsync(companyId, request.NewCompanyPhotos)
            : new List<string>();

        var finalKeys = keptOldKeys.Concat(newKeys).Distinct().ToList();

        existingCompany.SetPhotos(finalKeys);
        existingCompany.SetAuditUpdate(request.UpdatedById);

        _companyRepository.Update(existingCompany);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var toDeleteFromStorage = originalKeys.Except(finalKeys).ToList();
        if (toDeleteFromStorage.Count > 0)
        {
            await DeletePhotosAsync(companyId, toDeleteFromStorage);
        }

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

    private static ErrorOr<PhoneNumber> ValidatePhoneNumber(string phone, string countryCode)
    {
        try
        {
            return PhoneNumber.Create(phone, countryCode);
        }
        catch (ArgumentException ex)
        {
            return Error.Validation("UpdateCompany.PhoneNumber", ex.Message);
        }
    }

    private static ErrorOr<Address> ValidateAddress(UpdateCompanyCommand request)
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
            return Error.Validation("UpdateCompany.Address", ex.Message);
        }
    }

    private static ErrorOr<WorkSchedule> ValidateSchedule(UpdateCompanyCommand request)
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
            return Error.Validation("UpdateCompany.Schedule", ex.Message);
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
        // Limitar concurrencia si suben muchas
        using var gate = new SemaphoreSlim(4);

        var uploadTasks = photos.Select(async photo =>
        {
            await gate.WaitAsync();
            try
            {
                var ext = Path.GetExtension(photo.FileName);
                var safeExt = string.IsNullOrWhiteSpace(ext) ? ".png" : ext.ToLowerInvariant();
                var objectName = $"{companyId}/{Guid.NewGuid()}{safeExt}";

                var contentType = string.IsNullOrWhiteSpace(photo.ContentType) ? "image/png" : photo.ContentType;

                await using var stream = photo.OpenReadStream();
                var uploadResult = await _fileStorageService.UploadFileAsync(BUCKETNAME, objectName, stream, contentType);

                return uploadResult.IsError ? null : objectName;
            }
            finally { gate.Release(); }
        });

        var uploaded = await Task.WhenAll(uploadTasks);
        return uploaded.Where(k => k is not null).Cast<string>().ToList();
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
