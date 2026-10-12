using System.Linq.Expressions;

namespace Sefic.Partners.Shared.Filtro;

public abstract class Filtro<T>
{
    public virtual Expression<Func<T, bool>> BuildFilter()
    {
        return x => true;
    }
}
