using Framework.Application.Common;
using Web.Api.Application.Guilds.DTOs;
using Web.Api.Domain.Guild;

namespace Web.Api.Application.Guilds.Commands.PatchGuild
{
    public record PatchGuildCommand : BaseResponse<GuildResponseDTO>
    {
        public Guid Id { get; init; }
        public string? Name { get; init; }
        public string? Icon { get; init; }
        public string? Notice { get; init; }
        public IReadOnlyList<OnlineEnum>? Online { get; init; }
        public IReadOnlyList<TagEnum>? Tags { get; init; }
        public int? Level { get; init; }
        public int? TypeOfIncome { get; init; }
        public Guid? Master { get; init; }
        public Guid UpdatedById { get; init; }
    }
}
