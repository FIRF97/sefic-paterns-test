using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sefic.Partners.Shared.Enums;

namespace Sefic.Partners.Shared.Model;

public class Partner : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(9)]
    public string TaxNumber { get; set; } = string.Empty;

    [Required]
    public PartnerType Type { get; set; }

    [MaxLength(255)]
    public string? Email { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal CreditLimit { get; set; }

    [Timestamp]
    public byte[] Version { get; set; } = Array.Empty<byte>();
}
