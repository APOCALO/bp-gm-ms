using Framework.Application.Common;
using Web.Api.Application.Guilds.DTOs;
using Web.Api.Domain.Guild;

namespace Web.Api.Application.Guilds.Commands.UpdateGuild
{
    public record UpdateGuildCommand : BaseResponse<GuildResponseDTO>
    {
        public Guid Id { get; init; }
        public string Name { get; init; }
        public string Icon { get; init; }
        public string? Notice { get; init; }
        public IReadOnlyList<OnlineEnum> Online { get; init; } = Array.Empty<OnlineEnum>();
        public IReadOnlyList<TagEnum> Tags { get; init; } = Array.Empty<TagEnum>();
        public int Level { get; init; }
        public int TypeOfIncome { get; init; }
        public Guid Master { get; init; }
        public Guid UpdatedById { get; init; }
    }
}
