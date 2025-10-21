using Framework.Application.Interfaces.Repositories;
using GuildAgg = Web.Api.Domain.Guild.Guild;

namespace Web.Api.Application.Interfaces.Repositories
{
    public interface IGuildRepository : IBaseRepository<GuildAgg, Guid>
    {
        Task<bool> ExistsWithMasterAsync(Guid masterId, CancellationToken cancellationToken = default);
    }
}
