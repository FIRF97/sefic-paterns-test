using Sefic.Partners.Shared.Models;

namespace Sefic.Partners.Shared.Interface;

public interface IUriService
{
    Uri GetPageUri(PageFilter filter, string route);
    string BaseUri();
}
