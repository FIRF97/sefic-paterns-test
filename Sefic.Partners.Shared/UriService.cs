using Microsoft.AspNetCore.WebUtilities;
using Sefic.Partners.Shared.Interface;
using Sefic.Partners.Shared.Models;

namespace Sefic.Partners.Shared;

public class UriService : IUriService
{
    private readonly string _baseUri;

    public UriService(string baseUri)
    {
        _baseUri = baseUri.TrimEnd('/');
    }

    public Uri GetPageUri(PageFilter filter, string route)
    {
        var endpoint = string.Concat(_baseUri, route.StartsWith('/') ? route : "/" + route);
        var modifiedUri = QueryHelpers.AddQueryString(endpoint, "pageNumber", filter.PageNumber.ToString());
        modifiedUri = QueryHelpers.AddQueryString(modifiedUri, "pageSize", filter.PageSize.ToString());

        if (filter is PartnerFilter pf)
        {
            if (!string.IsNullOrWhiteSpace(pf.Search))
                modifiedUri = QueryHelpers.AddQueryString(modifiedUri, "search", pf.Search);

            if (!string.IsNullOrWhiteSpace(pf.Type))
                modifiedUri = QueryHelpers.AddQueryString(modifiedUri, "type", pf.Type);

            if (pf.IncludeInactive)
                modifiedUri = QueryHelpers.AddQueryString(modifiedUri, "includeInactive", "true");
        }
        
        return new Uri(modifiedUri);
    }

    public string BaseUri()
    {
        return _baseUri;
    }
}
