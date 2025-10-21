using AutoMapper;
using ErrorOr;
using Framework.Application.Common;
using Framework.Application.Interfaces;
using Framework.Domain.Primitives;
using Web.Api.Application.Interfaces.Repositories;
using Web.Api.Application.Players.DTOs;

namespace Web.Api.Application.Players.Commands.PatchPlayer
{
    internal sealed class PatchPlayerCommandHandler : ApiBaseHandler<PatchPlayerCommand, PlayerResponseDTO>
    {
        private readonly IUnitOfWork _uow;
        private readonly IPlayerRepository _repo;
        private readonly IMapper _mapper;

        public PatchPlayerCommandHandler(IUnitOfWork uow, IPlayerRepository repo, IMapper mapper, ILogger<PatchPlayerCommandHandler> logger) : base(logger)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        protected override async Task<ErrorOr<ApiResponse<PlayerResponseDTO>>> HandleRequest(PatchPlayerCommand request, CancellationToken cancellationToken)
        {
            var player = await _repo.GetByIdAsync(request.Id, cancellationToken);
            if (player is null)
            {
                return Error.NotFound("Player.NotFound", $"Player with ID {request.Id} not found.");
            }

            if (request.Name is not null) player.UpdateName(request.Name);
            if (request.Level.HasValue) player.UpdateLevel(request.Level.Value);
            if (request.GearScore.HasValue) player.UpdateGearScore(request.GearScore.Value);
            if (request.Position is not null) player.UpdatePosition(request.Position);
            if (request.ClassSpec.HasValue) player.UpdateClass(request.ClassSpec.Value);
            if (request.GuildId != null) player.AssignGuild(request.GuildId);
            player.SetAuditUpdate(request.UpdatedById);

            _repo.Update(player);
            await _uow.SaveChangesAsync(cancellationToken);

            var dto = _mapper.Map<PlayerResponseDTO>(player);
            return new ApiResponse<PlayerResponseDTO>(dto, true);
        }
    }
}
