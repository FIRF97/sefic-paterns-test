using System.Linq.Expressions;

namespace Sefic.Partners.Shared.Interface;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task<T?> GetOneByFilterAsync(Expression<Func<T, bool>>? filter = null, string[]? include = null);
    Task<ICollection<T>> GetByFilterAsync(Expression<Func<T, bool>>? filter = null, string[]? include = null);
    Task<ICollection<T>> GetAllWithoutFiltersAsync();
    Task AddAsync(T obj);
    Task AddRangeAsync(IEnumerable<T> entities);
    Task UpdateAsync(T obj);
    Task RemoveAsync(T obj);
    Task<int> CountAsync(Expression<Func<T, bool>>? filter = null);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> filter);
    Task<bool> AnyAsync(Expression<Func<T, bool>> filter);
    Task<IQueryable<T>> FindAsync(Expression<Func<T, bool>> filter);
    IQueryable<T> Query();
}
