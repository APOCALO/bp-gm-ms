using AutoMapper;
using ErrorOr;
using Framework.Application.Common;
using Framework.Domain.Primitives;
using Web.Api.Application.Interfaces.Repositories;
using Web.Api.Application.Players.DTOs;

namespace Web.Api.Application.Players.Commands.UpdatePlayer
{
    internal sealed class UpdatePlayerCommandHandler : ApiBaseHandler<UpdatePlayerCommand, PlayerResponseDTO>
    {
        private readonly IUnitOfWork _uow;
        private readonly IPlayerRepository _repo;
        private readonly IMapper _mapper;

        public UpdatePlayerCommandHandler(IUnitOfWork uow, IPlayerRepository repo, IMapper mapper, ILogger<UpdatePlayerCommandHandler> logger) : base(logger)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        protected override async Task<ErrorOr<ApiResponse<PlayerResponseDTO>>> HandleRequest(UpdatePlayerCommand request, CancellationToken cancellationToken)
        {
            var player = await _repo.GetByIdAsync(request.Id, cancellationToken);
            if (player is null)
            {
                return Error.NotFound("Player.NotFound", $"Player with ID {request.Id} not found.");
            }

            player.UpdateName(request.Name);
            player.UpdateLevel(request.Level);
            player.UpdateGearScore(request.GearScore);
            player.UpdatePosition(request.Position);
            player.UpdateClass(request.ClassSpec);
            player.AssignGuild(request.GuildId);
            player.SetAuditUpdate(request.UpdatedById);

            _repo.Update(player);
            await _uow.SaveChangesAsync(cancellationToken);

            var dto = _mapper.Map<PlayerResponseDTO>(player);
            return new ApiResponse<PlayerResponseDTO>(dto, true);
        }
    }
}
