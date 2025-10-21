using AutoMapper;
using ErrorOr;
using Framework.Application.Common;
using Web.Api.Application.Guilds.DTOs;
using Web.Api.Application.Interfaces.Repositories;

namespace Web.Api.Application.Guilds.Queries.GetAllGuildsPaged
{
    internal sealed class GetAllGuildsPagedQueryHandler : ApiBaseHandler<GetAllGuildsPagedQuery, IReadOnlyList<GuildResponseDTO>>
    {
        private readonly IMapper _mapper;
        private readonly IGuildRepository _repo;

        public GetAllGuildsPagedQueryHandler(IMapper mapper, ILogger<GetAllGuildsPagedQueryHandler> logger, IGuildRepository repo) : base(logger)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        protected override async Task<ErrorOr<ApiResponse<IReadOnlyList<GuildResponseDTO>>>> HandleRequest(GetAllGuildsPagedQuery request, CancellationToken cancellationToken)
        {
            var (items, total) = await _repo.GetPagedAsync(
                request.Pagination,
                filter: request.UserId is Guid uid ? g => g.CreatedById == uid : null,
                orderBy: q => q.OrderBy(g => g.Id),
                include: null,
                cancellationToken: cancellationToken);

            var dto = _mapper.Map<IReadOnlyList<GuildResponseDTO>>(items);
            var pagination = new PaginationMetadata
            {
                TotalCount = total,
                PageSize = request.Pagination.PageSize,
                PageNumber = request.Pagination.PageNumber
            };

            return new ApiResponse<IReadOnlyList<GuildResponseDTO>>(dto, true, pagination);
        }
    }
}
