using Domain.Companies;

namespace Application.Interfaces.Repositories
{
    public interface ICompanyRepository : IBaseRepository<Company, Guid>
    {
    }
}
