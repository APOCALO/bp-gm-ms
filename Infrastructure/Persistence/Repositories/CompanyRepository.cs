using Application.Interfaces.Repositories;
using Domain.Companies;
using Infrastructure.Persistence.Data;

namespace Infrastructure.Persistence.Repositories
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
