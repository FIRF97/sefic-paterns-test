namespace Sefic.Partners.Shared.Models;

public class PartnerFiltro : PageFilter
{
    public Guid? Id { get; set; }

    public string? Name { get; set; }
    
    public string? TaxNumber { get; set; }

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
    
    public string? OrderBy { get; set; }
    
    public bool? OrderByDescending { get; set; }

    public string? SortDirection { get; set; }
}
