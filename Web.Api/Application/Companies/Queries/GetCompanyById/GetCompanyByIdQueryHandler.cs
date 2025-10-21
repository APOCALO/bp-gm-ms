using AutoMapper;
using ErrorOr;
using Framework.Application.Common;
using Framework.Application.Interfaces;
using Web.Api.Application.Companies.DTOs;
using Web.Api.Application.Interfaces.Repositories;

namespace Web.Api.Application.Companies.Queries.GetCompanyById
{
    internal class GetCompanyByIdQueryHandler : ApiBaseHandler<GetCompanyByIdQuery, CompanyResponseDTO>
    {
        private readonly IMapper _mapper;
        private readonly ICompanyRepository _companyRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly IRedisCacheService _cache;

        private const string BUCKETNAME = "companies-photos";
        private const int URL_EXPIRY_SECONDS = 3600;

        public GetCompanyByIdQueryHandler(
            IMapper mapper,
            ICompanyRepository companyRepository,
            ILogger<GetCompanyByIdQueryHandler> logger,
            IFileStorageService fileStorageService,
            IRedisCacheService cache)
            : base(logger)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _companyRepository = companyRepository ?? throw new ArgumentNullException(nameof(companyRepository));
            _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        protected override async Task<ErrorOr<ApiResponse<CompanyResponseDTO>>> HandleRequest(
            GetCompanyByIdQuery request,
            CancellationToken cancellationToken)
        {
            // Obtener empresa
            var company = await _companyRepository.GetByIdAsync(request.Id, cancellationToken);
            if (company is null)
            {
                return Error.NotFound("Company.NotFound", "The company with the provided Id was not found.");
            }

            // Si no tiene fotos, devolvemos directamente
            if (company.CompanyPhotos == null || company.CompanyPhotos.Count == 0)
            {
                return new ApiResponse<CompanyResponseDTO>(_mapper.Map<CompanyResponseDTO>(company), true);
            }

            // Obtener URLs firmadas (intentando cache primero)
            var signedUrls = await GetSignedUrlsWithCacheAsync(company.Id, company.CompanyPhotos);

            // Asignar URLs solo si hay fotos válidas
            if (signedUrls.Count > 0)
                company.SetCoverPhotoUrls(signedUrls);

            // Mapear y devolver
            var mappedResult = _mapper.Map<CompanyResponseDTO>(company);
            return new ApiResponse<CompanyResponseDTO>(mappedResult, true);
        }

        #region Helpers

        private async Task<List<string>> GetSignedUrlsWithCacheAsync(Guid companyId, IReadOnlyCollection<string> photoKeys)
        {
            var cacheKey = $"v1:company:{companyId}:photos";

            // Intentar cache
            var cachedUrls = await _cache.GetAsync<List<string>>(cacheKey);
            if (cachedUrls is { Count: > 0 })
                return cachedUrls;

            // Generar URLs firmadas
            var signedUrlTasks = photoKeys.Select(async key =>
            {
                var urlResult = await _fileStorageService.GetFileUrlAsync(
                    BUCKETNAME,
                    key,
                    URL_EXPIRY_SECONDS
                );
                return urlResult.IsError ? null : urlResult.Value;
            });

            var signedUrls = (await Task.WhenAll(signedUrlTasks))
                .Where(url => url != null)
                .Cast<string>()
                .ToList();

            // Cachear solo si hay URLs válidas
            if (signedUrls.Count > 0)
            {
                var cacheTTL = TimeSpan.FromSeconds(URL_EXPIRY_SECONDS - 60);
                await _cache.SetAsync(cacheKey, signedUrls, cacheTTL);
            }

            return signedUrls;
        }

        #endregion
    }
}
