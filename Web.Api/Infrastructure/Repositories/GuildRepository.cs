using Microsoft.EntityFrameworkCore;
using Framework.Infrastructure.Persistence.Repositories;
using Web.Api.Application.Interfaces.Repositories;
using GuildAgg = Web.Api.Domain.Guild.Guild;
using Web.Api.Infrastructure.Data;

namespace Web.Api.Infrastructure.Repositories
{
    public class GuildRepository : BaseRepository<GuildAgg, Guid>, IGuildRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public GuildRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<bool> ExistsWithMasterAsync(Guid masterId, CancellationToken cancellationToken = default)
        {
            return _dbContext.Guilds.AnyAsync(g => g.MasterId == masterId, cancellationToken);
        }
    }
}
