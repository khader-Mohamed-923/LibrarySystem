using Microsoft.EntityFrameworkCore;
using LibrarySystem.Data.Context;
using LibrarySystem.Data.Entities;
using LibrarySystem.Data.Interfaces;

namespace LibrarySystem.Data.Repositories;

public class BookRepository : IBookRepository
{
    private readonly LibraryDbContext _context;

    public BookRepository(LibraryDbContext context)
    {
        _context = context;
    }

    public async Task<Book?> GetByIdAsync(int id)
    {
        return await _context.Books
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id);
    }


    public async Task<IReadOnlyList<Book>> GetAllAsync()
    {
        return await _context.Books
            .AsNoTracking()
            .ToListAsync();
    }


    public async Task AddAsync(Book book)
    {
        await _context.Books.AddAsync(book);
    }

    public void Update(Book book, byte[]? rowVersion = null)
    {
        _context.Books.Update(book);
        if (rowVersion is not null && rowVersion.Length > 0)
        {
            _context.Entry(book).OriginalValues[nameof(Book.RowVersion)] = rowVersion;
        }
    }

    public void Delete(Book book)
    {
        _context.Books.Remove(book);
    }

    public async Task<bool> IsIsbnUniqueAsync(string isbn, int? excludeId = null)
    {
        var query = _context.Books
            .AsNoTracking()
            .Where(b => b.ISBN == isbn);

        if (excludeId.HasValue)
        {
            query = query.Where(b => b.Id != excludeId.Value);
        }

        return !await query.AnyAsync();
    }
}
