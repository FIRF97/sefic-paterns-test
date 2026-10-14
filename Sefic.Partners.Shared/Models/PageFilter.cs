namespace Sefic.Partners.Shared.Models;

/// <summary>
/// Filtro base de paginação e ordenação genérico.
/// </summary>
public class PageFilter
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? OrderBy { get; set; }

    public PageFilter() { }

    public PageFilter(int pageNumber, int pageSize)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}
