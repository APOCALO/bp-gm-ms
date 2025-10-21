using AutoMapper;
using ErrorOr;
using Framework.Application.Common;
using Web.Api.Application.Interfaces.Repositories;
using Web.Api.Application.Players.DTOs;

namespace Web.Api.Application.Players.Queries.GetAllPlayersPaged
{
    internal sealed class GetAllPlayersPagedQueryHandler : ApiBaseHandler<GetAllPlayersPagedQuery, IReadOnlyList<PlayerResponseDTO>>
    {
        private readonly IMapper _mapper;
        private readonly IPlayerRepository _repo;

        public GetAllPlayersPagedQueryHandler(IMapper mapper, ILogger<GetAllPlayersPagedQueryHandler> logger, IPlayerRepository repo) : base(logger)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        protected override async Task<ErrorOr<ApiResponse<IReadOnlyList<PlayerResponseDTO>>>> HandleRequest(GetAllPlayersPagedQuery request, CancellationToken cancellationToken)
        {
            var (items, total) = await _repo.GetPagedAsync(
                request.Pagination,
                filter: request.UserId is Guid uid ? p => p.CreatedById == uid : null,
                orderBy: q => q.OrderBy(p => p.Id),
                include: null,
                cancellationToken: cancellationToken);

            var dto = _mapper.Map<IReadOnlyList<PlayerResponseDTO>>(items);
            var pagination = new PaginationMetadata
            {
                TotalCount = total,
                PageSize = request.Pagination.PageSize,
                PageNumber = request.Pagination.PageNumber
            };

            return new ApiResponse<IReadOnlyList<PlayerResponseDTO>>(dto, true, pagination);
        }
    }
}
