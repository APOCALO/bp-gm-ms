using Framework.Application.Common;
using Web.Api.Application.Players.DTOs;

namespace Web.Api.Application.Players.Queries.GetPlayerById
{
    public record GetPlayerByIdQuery : BaseResponse<PlayerResponseDTO>
    {
        public Guid Id { get; set; }

        public GetPlayerByIdQuery(Guid id)
        {
            Id = id;
        }
    }
}
