using Microsoft.EntityFrameworkCore;
using Sefic.Partners.Shared;
using Sefic.Partners.Shared.Interface;
using Sefic.Partners.Shared.Models;

namespace Sefic.Partners.Data;

public class PagedResponseBuilder : IPagedResponseBuilder
{
    private readonly IUriService? _uriService;

    public PagedResponseBuilder(IUriService? uriService = null)
    {
        _uriService = uriService;
    }

    public Task<PagedResult<T>> CreatePagedResultAsync<T>(IQueryable<T> query, int page, int pageSize)
    {
        return CreatePagedResultAsync(query, new PageFilter(page, pageSize));
    }

    public async Task<PagedResult<T>> CreatePagedResultAsync<T>(
        IQueryable<T> query,
        PageFilter filter,
        string route = "/api/partners",
        string mensagem = "Partner")
    {
        var validPage = filter.PageNumber < 1 ? 1 : filter.PageNumber;
        var validPageSize = filter.PageSize < 1 ? 20 : filter.PageSize;

        // Contar antes de paginar no SQL Server (conforme especificado no enunciado)
        var totalRecords = await query.CountAsync();

        // Paginação no banco
        var items = await query
            .Skip((validPage - 1) * validPageSize)
            .Take(validPageSize)
            .ToListAsync();

        var totalPages = validPageSize > 0 ? (int)Math.Ceiling(decimal.Divide(totalRecords, validPageSize)) : 0;

        var uriService = _uriService ?? new UriService("http://localhost:8080");
        var validTotalPages = totalPages < 1 ? 1 : totalPages;

        var nextPage = validPage < totalPages
            ? uriService.GetPageUri(CloneWithPage(filter, validPage + 1, validPageSize), route)
            : null;

        var previousPage = validPage > 1 && validPage <= totalPages + 1
            ? uriService.GetPageUri(CloneWithPage(filter, validPage - 1, validPageSize), route)
            : null;

        var firstPage = uriService.GetPageUri(CloneWithPage(filter, 1, validPageSize), route);
        var lastPage = uriService.GetPageUri(CloneWithPage(filter, validTotalPages, validPageSize), route);

        return new PagedResult<T>(
            items,
            totalRecords,
            validPage,
            validPageSize,
            totalPages,
            nextPage,
            previousPage,
            firstPage,
            lastPage,
            mensagem);
    }

    private static PageFilter CloneWithPage(PageFilter filter, int pageNumber, int pageSize)
    {
        if (filter is PartnerFilter pf)
        {
            return new PartnerFilter(pageNumber, pageSize)
            {
                Search = pf.Search,
                Type = pf.Type,
                IncludeInactive = pf.IncludeInactive,
                OrderBy = pf.OrderBy
            };
        }

        return new PageFilter(pageNumber, pageSize)
        {
            OrderBy = filter.OrderBy
        };
    }
}
