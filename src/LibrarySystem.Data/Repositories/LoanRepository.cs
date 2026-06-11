using LibrarySystem.Data.Context;
using LibrarySystem.Data.Entities;
using LibrarySystem.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Data.Repositories;

public class LoanRepository : ILoanRepository
{
    private readonly LibraryDbContext _context;

    public LoanRepository(LibraryDbContext context)
    {
        _context = context;
    }

    public async Task<Loan?> GetByIdAsync(int id)
    {
        return await _context.Loans
            .Include(l => l.Book)
            .Include(l => l.Member)
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task AddAsync(Loan loan)
    {
        await _context.Loans.AddAsync(loan);
    }

    public void Update(Loan loan)
    {
        _context.Loans.Update(loan);
    }

    public async Task<int> GetActiveLoanCountForMemberAsync(int memberId)
    {
        return await _context.Loans
            .CountAsync(l => l.MemberId == memberId && !l.IsReturned);
    }

    public async Task<bool> HasOverdueLoanAsync(int memberId)
    {
        var now = DateTime.UtcNow;
        return await _context.Loans
            .AnyAsync(l => l.MemberId == memberId && !l.IsReturned && l.DueDate < now);
    }

    public async Task<IEnumerable<Loan>> GetLoansByMemberAsync(int memberId)
    {
        return await _context.Loans
            .Include(l => l.Book)
            .Include(l => l.Member)
            .Where(l => l.MemberId == memberId)
            .OrderByDescending(l => l.LoanDate)
            .ToListAsync();
    }
}
