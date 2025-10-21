using Framework.Application.Interfaces.Repositories;
using PlayerAgg = Web.Api.Domain.Player.Player;

namespace Web.Api.Application.Interfaces.Repositories
{
    public interface IPlayerRepository : IBaseRepository<PlayerAgg, Guid>
    {
    }
}
