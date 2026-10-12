using System.ComponentModel.DataAnnotations;
using Sefic.Partners.Shared.Enums;

namespace Sefic.Partners.Services.DTO;

public class PartnerCreateDto
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "O NIF é obrigatório.")]
    public string TaxNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "O tipo é obrigatório.")]
    public PartnerType Type { get; set; }

    public string? Email { get; set; }

    [Required(ErrorMessage = "O limite de crédito é obrigatório.")]
    public decimal CreditLimit { get; set; }
}
