using Microsoft.EntityFrameworkCore;
using Sefic.Partners.Core;
using Sefic.Partners.Data.Context;
using Sefic.Partners.Shared.Interface;
using Sefic.Partners.Shared.Model;

namespace Sefic.Partners.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly PartnersDbContext _context;
    private IRepository<Partner>? _partnerRepository;

    public UnitOfWork(PartnersDbContext context)
    {
        _context = context;
    }

    public IRepository<Partner> PartnerRepository =>
        _partnerRepository ??= new Repository<Partner>(_context);

    public async Task<bool> CommitAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<int> CommitWithResultAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
