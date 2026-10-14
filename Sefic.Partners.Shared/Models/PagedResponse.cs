using System.Text.Json.Serialization;

namespace Sefic.Partners.Shared.Models;

public class PagedResponse<T> : Response<T>
{
    [JsonPropertyOrder(1)]
    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; }

    [JsonPropertyOrder(2)]
    [JsonPropertyName("pageNumber")]
    public int PageNumber { get; set; }

    [JsonPropertyOrder(3)]
    [JsonPropertyName("totalPages")]
    public int TotalPages { get; set; }

    [JsonPropertyOrder(4)]
    [JsonPropertyName("totalRecords")]
    public int TotalRecords { get; set; }

    [JsonPropertyOrder(5)]
    [JsonPropertyName("records")]
    public int Records { get; set; }

    [JsonPropertyOrder(6)]
    [JsonPropertyName("nextPage")]
    public Uri? NextPage { get; set; }

    [JsonPropertyOrder(7)]
    [JsonPropertyName("previousPage")]
    public Uri? PreviousPage { get; set; }

    [JsonPropertyOrder(8)]
    [JsonPropertyName("firstPage")]
    public Uri? FirstPage { get; set; }

    [JsonPropertyOrder(9)]
    [JsonPropertyName("lastPage")]
    public Uri? LastPage { get; set; }

    public PagedResponse() : base() { }

    public PagedResponse(
        T data,
        int pageNumber,
        int pageSize,
        int records,
        int totalRecords,
        int totalPages,
        Uri? nextPage = null,
        Uri? previousPage = null,
        Uri? firstPage = null,
        Uri? lastPage = null,
        string mensagem = "Partner") : base(data, new Retorno(200, mensagem))
    {
        Data = data;
        PageNumber = pageNumber;
        PageSize = pageSize;
        Records = records;
        TotalRecords = totalRecords;
        TotalPages = totalPages;
        NextPage = nextPage;
        PreviousPage = previousPage;
        FirstPage = firstPage;
        LastPage = lastPage;
        Retorno = new Retorno(200, mensagem);
    }
}
