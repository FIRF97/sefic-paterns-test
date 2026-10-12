using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Sefic.Partners.Shared.Enums;
using Sefic.Partners.Shared.Filtro;
using Sefic.Partners.Shared.Helper;
using Sefic.Partners.Shared.Model;

namespace Sefic.Partners.Shared.Models;

/// <summary>
/// Filtro avançado para a entidade Partner que segue o padrão de Filtro&lt;T&gt; do ms-core-fundcube.
/// Constrói a árvore de expressão LINQ diretamente via BuildFilter() para aplicação no banco de dados.
/// </summary>
public class PartnerFiltro : Filtro<Partner>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public bool IncludeInactive { get; set; } = false;

    public Guid? Id { get; set; }
    public string? Name { get; set; }
    public string? TaxNumber { get; set; }
    public string? Type { get; set; }
    public string? Email { get; set; }
    public decimal? CreditLimit { get; set; }
    public decimal? MinCreditLimit { get; set; }
    public decimal? MaxCreditLimit { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? CreatedAtFrom { get; set; }
    public DateTime? CreatedAtTo { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? UpdatedAtFrom { get; set; }
    public DateTime? UpdatedAtTo { get; set; }
    public string? Search { get; set; }
    public string? OrderBy { get; set; }
    public bool? OrderByDescending { get; set; }
    public string? SortDirection { get; set; }

    /// <summary>
    /// Constrói a expressão de filtro combinada (LINQ Expression) para ser aplicada diretamente no SQL Server via IRepository.
    /// </summary>
    public override Expression<Func<Partner, bool>> BuildFilter()
    {
        Expression<Func<Partner, bool>> filter = x => true;

        // 1. Filtro por Id
        if (Id.HasValue && Id.Value != Guid.Empty)
            filter = filter.AndAlso(p => p.Id == Id.Value);

        // 2. Filtro IsActive / IncludeInactive
        if (IsActive.HasValue)
            filter = filter.AndAlso(p => p.IsActive == IsActive.Value);
        else if (!IncludeInactive)
            filter = filter.AndAlso(p => p.IsActive);

        // 3. Filtro por Type (CUSTOMER inclui CUSTOMER e BOTH; SUPPLIER inclui SUPPLIER e BOTH; BOTH apenas BOTH)
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

        // 4. Filtro por Nome específico
        if (!string.IsNullOrWhiteSpace(Name))
        {
            var nameTrimmed = Name.Trim();
            filter = filter.AndAlso(p => EF.Functions.Like(p.Name, $"%{nameTrimmed}%"));
        }

        // 5. Filtro por NIF (TaxNumber) específico
        if (!string.IsNullOrWhiteSpace(TaxNumber))
        {
            var digits = new string(TaxNumber.Where(char.IsDigit).ToArray());
            if (!string.IsNullOrWhiteSpace(digits))
                filter = filter.AndAlso(p => p.TaxNumber.Contains(digits));
        }

        // 6. Filtro por Email específico
        if (!string.IsNullOrWhiteSpace(Email))
        {
            var emailTrimmed = Email.Trim();
            filter = filter.AndAlso(p => p.Email != null && EF.Functions.Like(p.Email, $"%{emailTrimmed}%"));
        }

        // 7. Filtro por CreditLimit exato e faixa (Min / Max)
        if (CreditLimit.HasValue)
            filter = filter.AndAlso(p => p.CreditLimit == CreditLimit.Value);

        if (MinCreditLimit.HasValue)
            filter = filter.AndAlso(p => p.CreditLimit >= MinCreditLimit.Value);

        if (MaxCreditLimit.HasValue)
            filter = filter.AndAlso(p => p.CreditLimit <= MaxCreditLimit.Value);

        // 8. Filtro por Data de Registo (CreatedAt)
        if (CreatedAt.HasValue)
        {
            var target = CreatedAt.Value.Date;
            var next = target.AddDays(1);
            filter = filter.AndAlso(p => p.CreatedAt >= target && p.CreatedAt < next);
        }

        if (CreatedAtFrom.HasValue)
            filter = filter.AndAlso(p => p.CreatedAt >= CreatedAtFrom.Value);

        if (CreatedAtTo.HasValue)
            filter = filter.AndAlso(p => p.CreatedAt <= CreatedAtTo.Value);

        // 9. Filtro por Data de Atualização (UpdatedAt)
        if (UpdatedAt.HasValue)
        {
            var target = UpdatedAt.Value.Date;
            var next = target.AddDays(1);
            filter = filter.AndAlso(p => p.UpdatedAt >= target && p.UpdatedAt < next);
        }

        if (UpdatedAtFrom.HasValue)
            filter = filter.AndAlso(p => p.UpdatedAt >= UpdatedAtFrom.Value);

        if (UpdatedAtTo.HasValue)
            filter = filter.AndAlso(p => p.UpdatedAt <= UpdatedAtTo.Value);

        // 10. Filtro global Search (Nome ou NIF)
        if (!string.IsNullOrWhiteSpace(Search))
        {
            var searchTrimmed = Search.Trim();
            var searchDigits = new string(searchTrimmed.Where(char.IsDigit).ToArray());

            if (searchDigits.Length > 0)
            {
                filter = filter.AndAlso(p => EF.Functions.Like(p.Name, $"%{searchTrimmed}%") || p.TaxNumber.Contains(searchDigits));
            }
            else
            {
                filter = filter.AndAlso(p => EF.Functions.Like(p.Name, $"%{searchTrimmed}%"));
            }
        }

        return filter;
    }
}
