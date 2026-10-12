using Sefic.Partners.Shared.Interface;
using Sefic.Partners.Shared.Model;

namespace Sefic.Partners.Core;

public interface IUnitOfWork : IDisposable
{
    IRepository<Partner> PartnerRepository { get; }

    Task<int> CommitWithResultAsync();
    Task<bool> CommitAsync();
}
