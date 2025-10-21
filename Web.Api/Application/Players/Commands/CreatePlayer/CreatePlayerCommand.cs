using Framework.Application.Common;
using Web.Api.Application.Players.DTOs;
using Web.Api.Domain.Player;

namespace Web.Api.Application.Players.Commands.CreatePlayer
{
    public record CreatePlayerCommand : BaseResponse<PlayerResponseDTO>
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string Name { get; init; }
        public int Level { get; init; }
        public int GearScore { get; init; }
        public string Position { get; init; }
        public BpClassSpec ClassSpec { get; init; }
        public Guid? GuildId { get; init; }
        public Guid CreatedById { get; init; }
    }
}
