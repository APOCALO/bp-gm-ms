using Framework.Application.Interfaces.Repositories;
using Web.Api.Domain.Companies;

namespace Web.Api.Application.Interfaces.Repositories
{
    public interface ICompanyRepository : IBaseRepository<Company, Guid>
    {
    }
}
