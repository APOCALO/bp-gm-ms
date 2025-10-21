using Framework.Infrastructure.Persistence.Repositories;
using Web.Api.Application.Interfaces.Repositories;
using Web.Api.Domain.Companies;
using Web.Api.Infrastructure.Data;

namespace Web.Api.Infrastructure.Repositories
{
    public class CompanyRepository : BaseRepository<Company, Guid>, ICompanyRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public CompanyRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
    }
}
