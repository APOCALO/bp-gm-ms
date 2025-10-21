using AutoMapper;
using ErrorOr;
using Framework.Application.Common;
using Framework.Application.Interfaces;
using Framework.Domain.Primitives;
using Web.Api.Application.Guilds.DTOs;
using Web.Api.Application.Interfaces.Repositories;

namespace Web.Api.Application.Guilds.Commands.UpdateGuild
{
    internal sealed class UpdateGuildCommandHandler : ApiBaseHandler<UpdateGuildCommand, GuildResponseDTO>
    {
        private readonly IUnitOfWork _uow;
        private readonly IGuildRepository _repo;
        private readonly IPlayerRepository _playerRepo;
        private readonly IMapper _mapper;

        public UpdateGuildCommandHandler(
            IUnitOfWork uow,
            IGuildRepository repo,
            IPlayerRepository playerRepo,
            IMapper mapper,
            ILogger<UpdateGuildCommandHandler> logger) : base(logger)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _playerRepo = playerRepo ?? throw new ArgumentNullException(nameof(playerRepo));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        protected override async Task<ErrorOr<ApiResponse<GuildResponseDTO>>> HandleRequest(UpdateGuildCommand request, CancellationToken cancellationToken)
        {
            var guild = await _repo.GetByIdAsync(request.Id, cancellationToken);
            if (guild is null)
            {
                return Error.NotFound("Guild.NotFound", $"Guild with ID {request.Id} not found.");
            }

            // Validate master (exists and unique) if changed
            if (request.Master != guild.MasterId)
            {
                var master = await _playerRepo.GetByIdAsync(request.Master, cancellationToken);
                if (master is null)
                {
                    return Error.NotFound("UpdateGuild.MasterNotFound", $"Player with ID {request.Master} not found.");
                }
                if (await _repo.ExistsWithMasterAsync(request.Master, cancellationToken))
                {
                    return Error.Conflict("UpdateGuild.MasterAlreadyAssigned", "The specified player is already master of another guild.");
                }
            }

            guild.UpdateName(request.Name);
            guild.UpdateIcon(request.Icon);
            guild.UpdateNotice(request.Notice);
            guild.SetOnline(request.Online);
            guild.SetTags(request.Tags);
            guild.UpdateLevel(request.Level);
            guild.UpdateTypeOfIncome(request.TypeOfIncome);
            guild.UpdateMaster(request.Master);
            guild.SetAuditUpdate(request.UpdatedById);

            _repo.Update(guild);
            await _uow.SaveChangesAsync(cancellationToken);

            var dto = _mapper.Map<GuildResponseDTO>(guild);
            return new ApiResponse<GuildResponseDTO>(dto, true);
        }
    }
}
