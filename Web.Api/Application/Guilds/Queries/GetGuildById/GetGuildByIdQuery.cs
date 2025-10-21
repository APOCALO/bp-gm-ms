using Framework.Application.Common;
using Web.Api.Application.Guilds.DTOs;

namespace Web.Api.Application.Guilds.Queries.GetGuildById
{
    public record GetGuildByIdQuery : BaseResponse<GuildResponseDTO>
    {
        public Guid Id { get; set; }

        public GetGuildByIdQuery(Guid id)
        {
            Id = id;
        }
    }
}
