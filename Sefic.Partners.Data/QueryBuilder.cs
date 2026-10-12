using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Sefic.Partners.Data;

public class QueryBuilder<T> where T : class
{
    private IQueryable<T> _query;

    public QueryBuilder(IQueryable<T> query)
    {
        _query = query.AsNoTracking();
    }

    public QueryBuilder<T> AsTracking()
    {
        _query = _query.AsTracking();
        return this;
    }

    public QueryBuilder<T> Include(params string[]? includes)
    {
        if (includes != null)
        {
            foreach (var include in includes)
            {
                if (!string.IsNullOrWhiteSpace(include))
                {
                    _query = _query.Include(include);
                }
            }
        }
        return this;
    }

    public QueryBuilder<T> Filter(Expression<Func<T, bool>>? filter)
    {
        if (filter != null)
        {
            _query = _query.Where(filter);
        }
        return this;
    }

    public IQueryable<T> Build()
    {
        return _query;
    }
}
