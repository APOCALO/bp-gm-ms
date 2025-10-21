using ErrorOr;
using Framework.Application.Common;
using Framework.Application.Interfaces;
using Framework.Domain.Primitives;
using MediatR;
using Web.Api.Application.Interfaces.Repositories;
using Web.Api.Domain.Companies;

namespace Web.Api.Application.Companies.Commands.DeleteCompany
{
    internal sealed class DeleteCompanyCommandHandler : ApiBaseHandler<DeleteCompanyCommand, Unit>
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileStorageService _fileStorageService;

        private const string BUCKETNAME = "companies-photos";

        public DeleteCompanyCommandHandler(ICompanyRepository companyRepository, IUnitOfWork unitOfWork, ILogger<DeleteCompanyCommandHandler> logger, IFileStorageService fileStorageService) : base(logger)
        {
            _companyRepository = companyRepository ?? throw new ArgumentNullException(nameof(companyRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
        }

        protected async override Task<ErrorOr<ApiResponse<Unit>>> HandleRequest(DeleteCompanyCommand request, CancellationToken cancellationToken)
        {
            if (await _companyRepository.GetByIdAsync(request.Id, cancellationToken) is not Company company)
            {
                return Error.NotFound("DeleteCompanyCommandHandler.NotFound", $"Company with ID {request.Id} was not found.");
            }

            if (company.CreatedById != request.UserId)
            {
                return Error.Forbidden("DeleteCompanyCommandHandler.Forbidden", "You do not have permission to delete this company.");
            }

            if (company.CompanyPhotos.Count > 0)
            {
                await DeletePhotosAsync(company.Id, company.CompanyPhotos);
            }

            _companyRepository.Delete(company);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new ApiResponse<Unit>(Unit.Value, true);

            return response;
        }

        #region Helpers

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

        #endregion
    }
}
