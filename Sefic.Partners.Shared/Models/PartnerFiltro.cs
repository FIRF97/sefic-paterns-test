namespace Sefic.Partners.Shared.Models;

/// <summary>
/// Mantido para retrocompatibilidade com código ou referências que utilizam o nome em português PartnerFiltro.
/// </summary>
public class PartnerFiltro : PartnerFilter
{
    public PartnerFiltro() { }

    public PartnerFiltro(int pageNumber, int pageSize) : base(pageNumber, pageSize) { }
}
