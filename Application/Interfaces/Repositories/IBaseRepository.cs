using Application.Common;
using System.Linq.Expressions;

namespace Application.Interfaces.Repositories
{
    public interface IBaseRepository<T, TId>
        where T : class
    {
        Task<(IReadOnlyList<T> Items, int TotalCount)> GetPagedAsync(
            PaginationParameters paginationParameters,
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            Func<IQueryable<T>, IQueryable<T>>? include = null,
            CancellationToken cancellationToken = default);

        Task AddAsync(T entity, CancellationToken cancellationToken);
        Task<T?> GetByIdAsync(TId id, CancellationToken cancellationToken);
        Task<List<T>> FindByConditionAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken);
        void Update(T entity);
        void Delete(T entity);
    }
}
