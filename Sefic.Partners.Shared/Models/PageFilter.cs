namespace Sefic.Partners.Shared.Models;

public class PageFilter
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Search { get; set; }
    public string? Type { get; set; }
    public bool IncludeInactive { get; set; } = false;
    public string? OrderBy { get; set; }

    public PageFilter() { }

    public PageFilter(int page, int pageSize)
    {
        Page = page;
        PageSize = pageSize;
    }
}
