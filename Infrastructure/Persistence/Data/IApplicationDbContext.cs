using Domain.Companies;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Data
{
    public interface IApplicationDbContext
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        // Borrar los DBSet que no se usen (los DBSet de Customers y Reservations son de ejemplo)
        DbSet<Company> Companies { get; set; }
    }
}
