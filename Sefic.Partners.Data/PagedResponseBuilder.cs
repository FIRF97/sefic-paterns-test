using Microsoft.EntityFrameworkCore;
using Sefic.Partners.Shared.Interface;
using Sefic.Partners.Shared.Models;

namespace Sefic.Partners.Data;

public class PagedResponseBuilder : IPagedResponseBuilder
{
    public async Task<PagedResult<T>> CreatePagedResultAsync<T>(IQueryable<T> query, int page, int pageSize)
    {
        var validPage = page < 1 ? 1 : page;
        var validPageSize = pageSize < 1 ? 20 : pageSize;

        // Contar antes de paginar no SQL Server (conforme especificado no enunciado)
        var totalRecords = await query.CountAsync();

        // Paginação no banco
        var items = await query
            .Skip((validPage - 1) * validPageSize)
            .Take(validPageSize)
            .ToListAsync();

        return new PagedResult<T>(items, totalRecords, validPage, validPageSize);
    }
}
