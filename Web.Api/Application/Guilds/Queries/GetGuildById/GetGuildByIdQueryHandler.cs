using AutoMapper;
using ErrorOr;
using Framework.Application.Common;
using Web.Api.Application.Guilds.DTOs;
using Web.Api.Application.Interfaces.Repositories;

namespace Web.Api.Application.Guilds.Queries.GetGuildById
{
    internal sealed class GetGuildByIdQueryHandler : ApiBaseHandler<GetGuildByIdQuery, GuildResponseDTO>
    {
        private readonly IMapper _mapper;
        private readonly IGuildRepository _repo;

        public GetGuildByIdQueryHandler(IMapper mapper, IGuildRepository repo, ILogger<GetGuildByIdQueryHandler> logger) : base(logger)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        protected override async Task<ErrorOr<ApiResponse<GuildResponseDTO>>> HandleRequest(GetGuildByIdQuery request, CancellationToken cancellationToken)
        {
            var item = await _repo.GetByIdAsync(request.Id, cancellationToken);
            if (item is null)
            {
                return Error.NotFound("Guild.NotFound", "The guild with the provided Id was not found.");
            }

            var dto = _mapper.Map<GuildResponseDTO>(item);
            return new ApiResponse<GuildResponseDTO>(dto, true);
        }
    }
}
