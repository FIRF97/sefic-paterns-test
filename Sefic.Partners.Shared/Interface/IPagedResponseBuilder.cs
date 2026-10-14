using Sefic.Partners.Shared.Models;

namespace Sefic.Partners.Shared.Interface;

public interface IPagedResponseBuilder
{
    Task<PagedResult<T>> CreatePagedResultAsync<T>(IQueryable<T> query, int page, int pageSize);
    Task<PagedResult<T>> CreatePagedResultAsync<T>(IQueryable<T> query, PageFilter filter, string route = "/api/partners", string mensagem = "Partner");
}
