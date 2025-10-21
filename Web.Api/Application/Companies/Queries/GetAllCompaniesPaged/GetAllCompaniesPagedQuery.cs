using Framework.Application.Common;
using Web.Api.Application.Companies.DTOs;

namespace Web.Api.Application.Companies.Queries.GetAllCompaniesPaged
{
    public record GetAllCompaniesPagedQuery : BaseResponse<IReadOnlyList<CompanyResponseDTO>>
    {
        public PaginationParameters Pagination { get; set; }
        public Guid? UserId { get; set; }

        public GetAllCompaniesPagedQuery(PaginationParameters pagination, Guid? userId)
        {
            Pagination = pagination;
            UserId = userId;
        }
    }
}
