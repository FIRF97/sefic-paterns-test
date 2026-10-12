namespace Sefic.Partners.Shared.Models;

/// <summary>
/// Filtro avançado para consulta de entidades (Partner), executado diretamente no banco de dados,
/// permitindo filtrar por todas as colunas da entidade e ordenar de forma crescente ou decrescente mediante a data de registo.
/// </summary>
public class PartnerFiltro : PageFilter
{
    /// <summary>
    /// Filtro pelo identificador único (Id).
    /// </summary>
    public Guid? Id { get; set; }

    /// <summary>
    /// Filtro por Nome (pesquisa parcial ou exata).
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Filtro por NIF (TaxNumber). Aceita formatado ou apenas dígitos.
    /// </summary>
    public string? TaxNumber { get; set; }

    /// <summary>
    /// Filtro por Email (pesquisa parcial ou exata).
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Filtro por Limite de Crédito exato.
    /// </summary>
    public decimal? CreditLimit { get; set; }

    /// <summary>
    /// Filtro por Limite de Crédito mínimo (CreditLimit &gt;= MinCreditLimit).
    /// </summary>
    public decimal? MinCreditLimit { get; set; }

    /// <summary>
    /// Filtro por Limite de Crédito máximo (CreditLimit &lt;= MaxCreditLimit).
    /// </summary>
    public decimal? MaxCreditLimit { get; set; }

    /// <summary>
    /// Filtro direto pelo estado de ativação (true = ativo, false = inativo).
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Filtro por Data de Registo exata (CreatedAt - mesmo dia).
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Filtro por Data de Registo inicial (CreatedAt &gt;= CreatedAtFrom).
    /// </summary>
    public DateTime? CreatedAtFrom { get; set; }

    /// <summary>
    /// Filtro por Data de Registo final (CreatedAt &lt;= CreatedAtTo).
    /// </summary>
    public DateTime? CreatedAtTo { get; set; }

    /// <summary>
    /// Filtro por Data de Atualização exata (UpdatedAt - mesmo dia).
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Filtro por Data de Atualização inicial (UpdatedAt &gt;= UpdatedAtFrom).
    /// </summary>
    public DateTime? UpdatedAtFrom { get; set; }

    /// <summary>
    /// Filtro por Data de Atualização final (UpdatedAt &lt;= UpdatedAtTo).
    /// </summary>
    public DateTime? UpdatedAtTo { get; set; }

    /// <summary>
    /// Campo pelo qual ordenar (por padrão ordena por CreatedAt quando especificado).
    /// </summary>
    public string? OrderBy { get; set; }

    /// <summary>
    /// Ordenação mediante a data de registo (CreatedAt):
    /// true = decrescente (mais recente primeiro), false = crescente (mais antigo primeiro).
    /// </summary>
    public bool? OrderByDescending { get; set; }

    /// <summary>
    /// Direção da ordenação: "asc" / "crescente" ou "desc" / "decrescente".
    /// </summary>
    public string? SortDirection { get; set; }
}
