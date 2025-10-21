using ErrorOr;
using Framework.Application.Common;
using Framework.Domain.Primitives;
using MediatR;
using Web.Api.Application.Interfaces.Repositories;

namespace Web.Api.Application.Guilds.Commands.DeleteGuild
{
    internal sealed class DeleteGuildCommandHandler : ApiBaseHandler<DeleteGuildCommand, Unit>
    {
        private readonly IGuildRepository _repo;
        private readonly IUnitOfWork _uow;

        public DeleteGuildCommandHandler(IGuildRepository repo, IUnitOfWork uow, ILogger<DeleteGuildCommandHandler> logger) : base(logger)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        protected async override Task<ErrorOr<ApiResponse<Unit>>> HandleRequest(DeleteGuildCommand request, CancellationToken cancellationToken)
        {
            var guild = await _repo.GetByIdAsync(request.Id, cancellationToken);
            if (guild is null)
            {
                return Error.NotFound("Guild.NotFound", $"Guild with ID {request.Id} not found.");
            }

            if (guild.CreatedById != request.UserId)
            {
                return Error.Forbidden("Guild.Forbidden", "You do not have permission to delete this guild.");
            }

            _repo.Delete(guild);
            await _uow.SaveChangesAsync(cancellationToken);
            return new ApiResponse<Unit>(Unit.Value, true);
        }
    }
}
