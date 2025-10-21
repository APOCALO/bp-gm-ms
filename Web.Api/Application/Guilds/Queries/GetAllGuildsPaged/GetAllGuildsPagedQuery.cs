using Framework.Application.Common;
using Web.Api.Application.Guilds.DTOs;

namespace Web.Api.Application.Guilds.Queries.GetAllGuildsPaged
{
    public record GetAllGuildsPagedQuery : BaseResponse<IReadOnlyList<GuildResponseDTO>>
    {
        public PaginationParameters Pagination { get; set; }
        public Guid? UserId { get; set; }

        public GetAllGuildsPagedQuery(PaginationParameters pagination, Guid? userId)
        {
            Pagination = pagination;
            UserId = userId;
        }
    }
}
