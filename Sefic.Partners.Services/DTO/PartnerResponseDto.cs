using Sefic.Partners.Shared.Enums;
using Sefic.Partners.Shared.Model;

namespace Sefic.Partners.Services.DTO;

public class PartnerResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string TaxNumber { get; set; } = string.Empty;
    public PartnerType Type { get; set; }
    public string? Email { get; set; }
    public decimal CreditLimit { get; set; }
    public bool IsActive { get; set; }
    public string Version { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public static PartnerResponseDto FromEntity(Partner entity)
    {
        return new PartnerResponseDto
        {
            Id = entity.Id,
            Name = entity.Name,
            TaxNumber = entity.TaxNumber,
            Type = entity.Type,
            Email = entity.Email,
            CreditLimit = entity.CreditLimit,
            IsActive = entity.IsActive,
            Version = entity.Version != null && entity.Version.Length > 0
                ? Convert.ToBase64String(entity.Version)
                : string.Empty,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}
