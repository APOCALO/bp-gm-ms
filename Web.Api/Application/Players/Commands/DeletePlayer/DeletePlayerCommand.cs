using Framework.Application.Common;

using MediatR;

namespace Web.Api.Application.Players.Commands.DeletePlayer
{
    public record DeletePlayerCommand : BaseResponse<Unit>
    {
        public Guid Id { get; init; }
        public Guid UserId { get; init; }

        public DeletePlayerCommand(Guid id, Guid userId)
        {
            Id = id;
            UserId = userId;
        }
    }
}

