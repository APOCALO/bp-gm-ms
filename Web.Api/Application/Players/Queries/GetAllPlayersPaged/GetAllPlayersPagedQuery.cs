using Framework.Application.Common;
using Web.Api.Application.Players.DTOs;

namespace Web.Api.Application.Players.Queries.GetAllPlayersPaged
{
    public record GetAllPlayersPagedQuery : BaseResponse<IReadOnlyList<PlayerResponseDTO>>
    {
        public PaginationParameters Pagination { get; set; }
        public Guid? UserId { get; set; }

        public GetAllPlayersPagedQuery(PaginationParameters pagination, Guid? userId)
        {
            Pagination = pagination;
            UserId = userId;
        }
    }
}
