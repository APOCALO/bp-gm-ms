using Framework.Application.Common;
using MediatR;

namespace Web.Api.Application.Companies.Commands.DeleteCompany
{
    public record DeleteCompanyCommand : BaseResponse<Unit>
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }

        public DeleteCompanyCommand(Guid id, Guid userId)
        {
            Id = id;
            UserId = userId;
        }
    }
}
