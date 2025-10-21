using Framework.Application.Common;

using MediatR;

namespace Web.Api.Application.Guilds.Commands.DeleteGuild
{
    public record DeleteGuildCommand : BaseResponse<Unit>
    {
        public Guid Id { get; init; }
        public Guid UserId { get; init; }

        public DeleteGuildCommand(Guid id, Guid userId)
        {
            Id = id;
            UserId = userId;
        }
    }
}

