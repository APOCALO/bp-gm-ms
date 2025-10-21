using AutoMapper;
using ErrorOr;
using Framework.Application.Common;
using Web.Api.Application.Interfaces.Repositories;
using Web.Api.Application.Players.DTOs;

namespace Web.Api.Application.Players.Queries.GetPlayerById
{
    internal sealed class GetPlayerByIdQueryHandler : ApiBaseHandler<GetPlayerByIdQuery, PlayerResponseDTO>
    {
        private readonly IMapper _mapper;
        private readonly IPlayerRepository _repo;

        public GetPlayerByIdQueryHandler(IMapper mapper, IPlayerRepository repo, ILogger<GetPlayerByIdQueryHandler> logger) : base(logger)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        protected override async Task<ErrorOr<ApiResponse<PlayerResponseDTO>>> HandleRequest(GetPlayerByIdQuery request, CancellationToken cancellationToken)
        {
            var item = await _repo.GetByIdAsync(request.Id, cancellationToken);
            if (item is null)
            {
                return Error.NotFound("Player.NotFound", "The player with the provided Id was not found.");
            }

            var dto = _mapper.Map<PlayerResponseDTO>(item);
            return new ApiResponse<PlayerResponseDTO>(dto, true);
        }
    }
}
