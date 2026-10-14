using System.Text.Json.Serialization;

namespace Sefic.Partners.Shared.Models;

public class PagedResult<T> : PagedResponse<IReadOnlyCollection<T>>
{
    public PagedResult() : base()
    {
        Data = new List<T>();
    }

    public PagedResult(
        IReadOnlyCollection<T> items,
        int totalRecords,
        int pageNumber,
        int pageSize,
        int totalPages = 0,
        Uri? nextPage = null,
        Uri? previousPage = null,
        Uri? firstPage = null,
        Uri? lastPage = null,
        string mensagem = "Partner")
        : base(
            items ?? new List<T>(),
            pageNumber,
            pageSize,
            items?.Count ?? 0,
            totalRecords,
            totalPages > 0 ? totalPages : (pageSize > 0 ? (int)Math.Ceiling(decimal.Divide(totalRecords, pageSize)) : 0),
            nextPage,
            previousPage,
            firstPage,
            lastPage,
            mensagem)
    {
    }

    // Compatibilidade com código/testes legados que usam .Items, .TotalCount, .Page:
    [JsonIgnore]
    public IReadOnlyCollection<T> Items
    {
        get => Data;
        set => Data = value;
    }

    [JsonIgnore]
    public int TotalCount
    {
        get => TotalRecords;
        set => TotalRecords = value;
    }

    [JsonIgnore]
    public int Page
    {
        get => PageNumber;
        set => PageNumber = value;
    }
}
