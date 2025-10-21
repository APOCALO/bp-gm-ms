using AutoMapper;
using ErrorOr;
using Framework.Application.Common;
using Framework.Application.Interfaces;
using Framework.Domain.Primitives;
using Web.Api.Application.Guilds.DTOs;
using Web.Api.Application.Interfaces.Repositories;

namespace Web.Api.Application.Guilds.Commands.PatchGuild
{
    internal sealed class PatchGuildCommandHandler : ApiBaseHandler<PatchGuildCommand, GuildResponseDTO>
    {
        private readonly IUnitOfWork _uow;
        private readonly IGuildRepository _repo;
        private readonly IPlayerRepository _playerRepo;
        private readonly IMapper _mapper;

        public PatchGuildCommandHandler(
            IUnitOfWork uow,
            IGuildRepository repo,
            IPlayerRepository playerRepo,
            IMapper mapper,
            ILogger<PatchGuildCommandHandler> logger) : base(logger)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _playerRepo = playerRepo ?? throw new ArgumentNullException(nameof(playerRepo));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        protected override async Task<ErrorOr<ApiResponse<GuildResponseDTO>>> HandleRequest(PatchGuildCommand request, CancellationToken cancellationToken)
        {
            var guild = await _repo.GetByIdAsync(request.Id, cancellationToken);
            if (guild is null)
            {
                return Error.NotFound("Guild.NotFound", $"Guild with ID {request.Id} not found.");
            }

            if (request.Master.HasValue && request.Master.Value != guild.MasterId)
            {
                var master = await _playerRepo.GetByIdAsync(request.Master.Value, cancellationToken);
                if (master is null)
                {
                    return Error.NotFound("PatchGuild.MasterNotFound", $"Player with ID {request.Master.Value} not found.");
                }
                if (await _repo.ExistsWithMasterAsync(request.Master.Value, cancellationToken))
                {
                    return Error.Conflict("PatchGuild.MasterAlreadyAssigned", "The specified player is already master of another guild.");
                }
            }

            if (request.Name is not null) guild.UpdateName(request.Name);
            if (request.Icon is not null) guild.UpdateIcon(request.Icon);
            if (request.Notice is not null) guild.UpdateNotice(request.Notice);
            if (request.Online is not null) guild.SetOnline(request.Online);
            if (request.Tags is not null) guild.SetTags(request.Tags);
            if (request.Level.HasValue) guild.UpdateLevel(request.Level.Value);
            if (request.TypeOfIncome.HasValue) guild.UpdateTypeOfIncome(request.TypeOfIncome.Value);
            if (request.Master.HasValue) guild.UpdateMaster(request.Master.Value);

            guild.SetAuditUpdate(request.UpdatedById);

            _repo.Update(guild);
            await _uow.SaveChangesAsync(cancellationToken);

            var dto = _mapper.Map<GuildResponseDTO>(guild);
            return new ApiResponse<GuildResponseDTO>(dto, true);
        }
    }
}
