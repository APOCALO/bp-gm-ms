using Application.Common;
using Application.Extensions;
using Application.Interfaces.Repositories;
using ErrorOr;
using Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Persistence.Repositories
{
    public class BaseRepository<T, TId> : IBaseRepository<T, TId>
        where T : class
    {
        private readonly ApplicationDbContext _dbContext;

        public BaseRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task AddAsync(T entity, CancellationToken cancellationToken)
        {
            await _dbContext.Set<T>().AddAsync(entity, cancellationToken);
        }

        public void Delete(T entity)
        {
            _dbContext.Set<T>().Remove(entity);
        }

        public async Task<List<T>> FindByConditionAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
        {
            return await _dbContext.Set<T>()
                .Where(predicate)
                .ToListAsync(cancellationToken);
        }
            
        public async Task<T?> GetByIdAsync(TId id, CancellationToken cancellationToken)
        {
            return await _dbContext.Set<T>().FindAsync(id, cancellationToken);
        }

        public async Task<(IReadOnlyList<T> Items, int TotalCount)> GetPagedAsync(
            PaginationParameters paginationParameters,
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            Func<IQueryable<T>, IQueryable<T>>? include = null,
            CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = _dbContext.Set<T>().AsNoTracking();

            if (include is not null)
                query = include(query);

            if (filter is not null)
                query = query.Where(filter);

            // el total es del query filtrado
            var totalCount = await query.CountAsync(cancellationToken);

            if (orderBy is not null)
            {
                query = orderBy(query);
            }
            else
            {
                // Fallback: ordenar por PK si existe, para orden estable
                var entityType = _dbContext.Model.FindEntityType(typeof(T));
                var keyName = entityType?.FindPrimaryKey()?.Properties.FirstOrDefault()?.Name;
                if (!string.IsNullOrEmpty(keyName))
                {
                    query = query.OrderBy(e => EF.Property<object>(e, keyName!));
                }
            }

            var items = await query
                .Paginate(paginationParameters)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public void Update(T entity)
        {
            if (entity == null)
            {
                Error.Validation("BaseRepository.Update", $"entity {typeof(T)} cannot be null.");
                return;
            }

            _dbContext.Entry(entity).State = EntityState.Modified;
        }
    }
}
