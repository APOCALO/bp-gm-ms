using Framework.Application.Common;
using Web.Api.Application.Companies.DTOs;

namespace Web.Api.Application.Companies.Queries.GetCompanyById
{
    public record GetCompanyByIdQuery : BaseResponse<CompanyResponseDTO>
    {
        public Guid Id { get; set; }

        public GetCompanyByIdQuery(Guid id)
        {
            Id = id;
        }
    }
}
