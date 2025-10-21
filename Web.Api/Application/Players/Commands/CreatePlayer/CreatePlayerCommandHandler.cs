using AutoMapper;
using ErrorOr;
using Framework.Application.Common;
using Framework.Domain.Primitives;
using Web.Api.Application.Interfaces.Repositories;
using Web.Api.Application.Players.DTOs;
using PlayerAgg = Web.Api.Domain.Player.Player;

namespace Web.Api.Application.Players.Commands.CreatePlayer
{
    internal sealed class CreatePlayerCommandHandler : ApiBaseHandler<CreatePlayerCommand, PlayerResponseDTO>
    {
        private readonly IUnitOfWork _uow;
        private readonly IPlayerRepository _repo;
        private readonly IMapper _mapper;

        public CreatePlayerCommandHandler(IUnitOfWork uow, IPlayerRepository repo, IMapper mapper, ILogger<CreatePlayerCommandHandler> logger) : base(logger)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        protected override async Task<ErrorOr<ApiResponse<PlayerResponseDTO>>> HandleRequest(CreatePlayerCommand request, CancellationToken cancellationToken)
        {
            var player = PlayerAgg.Create(
                createdById: request.CreatedById,
                name: request.Name,
                level: request.Level,
                gearScore: request.GearScore,
                position: request.Position,
                classSpec: request.ClassSpec,
                guildId: request.GuildId,
                id: request.Id == Guid.Empty ? null : request.Id);

            await _repo.AddAsync(player, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            var dto = _mapper.Map<PlayerResponseDTO>(player);
            return new ApiResponse<PlayerResponseDTO>(dto, true);
        }
    }
}
