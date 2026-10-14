using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Sefic.Partners.Shared.Enums;
using Sefic.Partners.Shared.Helper;
using Sefic.Partners.Shared.Model;

namespace Sefic.Partners.Shared.Models;

public class PartnerFilter : PageFilter
{
    public string? Search { get; set; }
    public string? Type { get; set; }
    public bool IncludeInactive { get; set; } = false;

    public PartnerFilter() { }

    public PartnerFilter(int pageNumber, int pageSize) : base(pageNumber, pageSize) { }

    public Expression<Func<Partner, bool>> BuildFilter()
    {
        Expression<Func<Partner, bool>> filter = x => true;

        if (!IncludeInactive)
        {
            filter = filter.AndAlso(p => p.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(Type) && Enum.TryParse<PartnerType>(Type.Trim(), true, out var parsedType))
        {
            filter = parsedType switch
            {
                PartnerType.CUSTOMER => filter.AndAlso(p => p.Type == PartnerType.CUSTOMER || p.Type == PartnerType.BOTH),
                PartnerType.SUPPLIER => filter.AndAlso(p => p.Type == PartnerType.SUPPLIER || p.Type == PartnerType.BOTH),
                PartnerType.BOTH => filter.AndAlso(p => p.Type == PartnerType.BOTH),
                _ => filter
            };
        }

        if (!string.IsNullOrWhiteSpace(Search))
        {
            var searchTrimmed = Search.Trim();
            var nifDigits = new string(searchTrimmed.Where(char.IsDigit).ToArray());

            if (!string.IsNullOrWhiteSpace(nifDigits))
            {
                filter = filter.AndAlso(p => EF.Functions.Like(p.Name, $"%{searchTrimmed}%") || p.TaxNumber.Contains(nifDigits));
            }
            else
            {
                filter = filter.AndAlso(p => EF.Functions.Like(p.Name, $"%{searchTrimmed}%"));
            }
        }

        return filter;
    }
}
