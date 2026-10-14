using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Sefic.Partners.Shared.Context;
using Sefic.Partners.Shared.Interface;
using Sefic.Partners.Shared.Models;

namespace Sefic.Partners.Data;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly PartnersDbContext _context;
    private readonly DbSet<T> _dbc;
    private readonly IPagedResponseBuilder _pagedResponseBuilder;

    public Repository(PartnersDbContext context, IPagedResponseBuilder? pagedResponseBuilder = null)
    {
        _context = context;
        _dbc = _context.Set<T>();
        _pagedResponseBuilder = pagedResponseBuilder ?? new PagedResponseBuilder();
    }

    public async Task<PagedResult<T>> GetAllAsync(
        PageFilter filter,
        string? orderBy = null,
        string[]? includes = null,
        Expression<Func<T, bool>>? filtrosPropriedades = null,
        string route = "/api/partners",
        string mensagem = "Partner")
    {
        IQueryable<T> query = new QueryBuilder<T>(_dbc)
            .Include(includes)
            .Filter(filtrosPropriedades)
            .OrderBy(orderBy)
            .Build();

        return await _pagedResponseBuilder.CreatePagedResultAsync(query, filter, route, mensagem);
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbc.FindAsync(id);
    }

    public async Task<T?> GetOneByFilterAsync(Expression<Func<T, bool>>? filter = null, string[]? include = null)
    {
        IQueryable<T> query = _dbc.AsNoTracking();

        if (include != null)
        {
            foreach (var inc in include)
            {
                if (!string.IsNullOrWhiteSpace(inc))
                    query = query.Include(inc);
            }
        }

        if (filter != null)
        {
            query = query.Where(filter);
        }

        return await query.FirstOrDefaultAsync();
    }

    public async Task<ICollection<T>> GetByFilterAsync(Expression<Func<T, bool>>? filter = null, string[]? include = null)
    {
        IQueryable<T> query = _dbc.AsNoTracking();

        if (include != null)
        {
            foreach (var inc in include)
            {
                if (!string.IsNullOrWhiteSpace(inc))
                    query = query.Include(inc);
            }
        }

        if (filter != null)
        {
            query = query.Where(filter);
        }

        return await query.ToListAsync();
    }

    public async Task<ICollection<T>> GetAllWithoutFiltersAsync()
    {
        return await _dbc.AsNoTracking().ToListAsync();
    }

    public async Task AddAsync(T obj)
    {
        await _dbc.AddAsync(obj);
    }

    public async Task AddRangeAsync(IEnumerable<T> entities)
    {
        await _dbc.AddRangeAsync(entities);
    }

    public Task UpdateAsync(T obj)
    {
        _dbc.Update(obj);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(T obj)
    {
        _dbc.Remove(obj);
        return Task.CompletedTask;
    }

    public async Task<int> CountAsync(Expression<Func<T, bool>>? filter = null)
    {
        return filter == null ? await _dbc.CountAsync() : await _dbc.CountAsync(filter);
    }

    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> filter)
    {
        return await _dbc.AsNoTracking().AnyAsync(filter);
    }

    public async Task<bool> AnyAsync(Expression<Func<T, bool>> filter)
    {
        return await _dbc.AsNoTracking().AnyAsync(filter);
    }

    public Task<IQueryable<T>> FindAsync(Expression<Func<T, bool>> filter)
    {
        return Task.FromResult(_dbc.AsNoTracking().Where(filter));
    }

    public IQueryable<T> Query()
    {
        return _dbc.AsQueryable();
    }
}
