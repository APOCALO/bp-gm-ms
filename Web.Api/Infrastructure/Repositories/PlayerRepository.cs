using Framework.Infrastructure.Persistence.Repositories;
using Web.Api.Application.Interfaces.Repositories;
using PlayerAgg = Web.Api.Domain.Player.Player;
using Web.Api.Infrastructure.Data;

namespace Web.Api.Infrastructure.Repositories
{
    public class PlayerRepository : BaseRepository<PlayerAgg, Guid>, IPlayerRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public PlayerRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
    }
}
