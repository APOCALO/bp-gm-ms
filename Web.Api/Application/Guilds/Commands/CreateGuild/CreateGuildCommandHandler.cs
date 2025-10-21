using AutoMapper;
using ErrorOr;
using Framework.Application.Common;
using Framework.Domain.Primitives;
using Web.Api.Application.Guilds.DTOs;
using Web.Api.Application.Interfaces.Repositories;
using GuildAgg = Web.Api.Domain.Guild.Guild;

namespace Web.Api.Application.Guilds.Commands.CreateGuild
{
    internal sealed class CreateGuildCommandHandler : ApiBaseHandler<CreateGuildCommand, GuildResponseDTO>
    {
        private readonly IUnitOfWork _uow;
        private readonly IGuildRepository _repo;
        private readonly IPlayerRepository _playerRepo;
        private readonly IMapper _mapper;

        public CreateGuildCommandHandler(
            IUnitOfWork uow,
            IGuildRepository repo,
            IPlayerRepository playerRepo,
            IMapper mapper,
            ILogger<CreateGuildCommandHandler> logger) : base(logger)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _playerRepo = playerRepo ?? throw new ArgumentNullException(nameof(playerRepo));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        protected override async Task<ErrorOr<ApiResponse<GuildResponseDTO>>> HandleRequest(CreateGuildCommand request, CancellationToken cancellationToken)
        {
            // validate master exists and is not already master elsewhere
            var master = await _playerRepo.GetByIdAsync(request.Master, cancellationToken);
            if (master is null)
            {
                return Error.NotFound("CreateGuild.MasterNotFound", $"Player with ID {request.Master} not found.");
            }

            if (await _repo.ExistsWithMasterAsync(request.Master, cancellationToken))
            {
                return Error.Conflict("CreateGuild.MasterAlreadyAssigned", "The specified player is already master of another guild.");
            }

            var guild = GuildAgg.Create(
                createdById: request.CreatedById,
                name: request.Name,
                icon: request.Icon,
                notice: request.Notice,
                online: request.Online,
                tags: request.Tags,
                level: request.Level,
                typeOfIncome: request.TypeOfIncome,
                masterId: request.Master,
                id: request.Id == Guid.Empty ? null : request.Id);

            await _repo.AddAsync(guild, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            var dto = _mapper.Map<GuildResponseDTO>(guild);
            return new ApiResponse<GuildResponseDTO>(dto, true);
        }
    }
}
