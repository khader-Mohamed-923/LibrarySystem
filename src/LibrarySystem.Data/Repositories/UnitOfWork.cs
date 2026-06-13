using LibrarySystem.Data.Context;
using LibrarySystem.Data.Interfaces;

namespace LibrarySystem.Data.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly LibraryDbContext _context;

    public UnitOfWork(LibraryDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Database.BeginTransactionAsync(cancellationToken);
    }
}
