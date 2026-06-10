using Microsoft.EntityFrameworkCore;
using LibrarySystem.Data.Context;
using LibrarySystem.Data.Entities;
using LibrarySystem.Data.Interfaces;

namespace LibrarySystem.Data.Repositories;

public class MemberRepository : IMemberRepository
{
    private readonly LibraryDbContext _context;

    public MemberRepository(LibraryDbContext context)
    {
        _context = context;
    }

    public async Task<Member?> GetByIdAsync(int id)
    {
        return await _context.Members
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<IReadOnlyList<Member>> GetAllAsync()
    {
        return await _context.Members
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddAsync(Member member)
    {
        await _context.Members.AddAsync(member);
    }

    public void Update(Member member, byte[]? rowVersion = null)
    {
        _context.Members.Update(member);
        if (rowVersion is not null && rowVersion.Length > 0)
        {
            _context.Entry(member).OriginalValues[nameof(Member.RowVersion)] = rowVersion;
        }
    }

    public void Delete(Member member)
    {
        _context.Members.Remove(member);
    }

    public async Task<bool> IsEmailUniqueAsync(string email, int? excludeId = null)
    {
        var query = _context.Members
            .AsNoTracking()
            .Where(m => m.Email == email);

        if (excludeId.HasValue)
        {
            query = query.Where(m => m.Id != excludeId.Value);
        }

        return !await query.AnyAsync();
    }
}
