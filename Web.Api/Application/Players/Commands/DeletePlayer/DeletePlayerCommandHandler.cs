using ErrorOr;
using Framework.Application.Common;
using Framework.Application.Interfaces;
using Framework.Domain.Primitives;
using MediatR;
using Web.Api.Application.Interfaces.Repositories;

namespace Web.Api.Application.Players.Commands.DeletePlayer
{
    internal sealed class DeletePlayerCommandHandler : ApiBaseHandler<DeletePlayerCommand, Unit>
    {
        private readonly IPlayerRepository _repo;
        private readonly IGuildRepository _guildRepo;
        private readonly IUnitOfWork _uow;

        public DeletePlayerCommandHandler(IPlayerRepository repo, IGuildRepository guildRepo, IUnitOfWork uow, ILogger<DeletePlayerCommandHandler> logger) : base(logger)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _guildRepo = guildRepo ?? throw new ArgumentNullException(nameof(guildRepo));
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        protected async override Task<ErrorOr<ApiResponse<Unit>>> HandleRequest(DeletePlayerCommand request, CancellationToken cancellationToken)
        {
            var player = await _repo.GetByIdAsync(request.Id, cancellationToken);
            if (player is null)
            {
                return Error.NotFound("Player.NotFound", $"Player with ID {request.Id} not found.");
            }

            if (player.CreatedById != request.UserId)
            {
                return Error.Forbidden("Player.Forbidden", "You do not have permission to delete this player.");
            }

            // Prevent deleting a player who is master of any guild
            if (await _guildRepo.ExistsWithMasterAsync(request.Id, cancellationToken))
            {
                return Error.Conflict("Player.MasterOfGuild", "Cannot delete player because they are the master of a guild.");
            }

            _repo.Delete(player);
            await _uow.SaveChangesAsync(cancellationToken);
            return new ApiResponse<Unit>(Unit.Value, true);
        }
    }
}
