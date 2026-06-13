using LibrarySystem.Data.Entities;

namespace LibrarySystem.Data.Interfaces;

public interface IBookRepository
{
    Task<Book?> GetByIdAsync(int id, bool track = false);
    Task<Book?> GetByIdWithUpdLockAsync(int id);
    Task<IReadOnlyList<Book>> GetAllAsync();
    Task AddAsync(Book book);
    void Update(Book book, byte[]? rowVersion = null);
    void Delete(Book book);
    Task<bool> IsIsbnUniqueAsync(string isbn, int? excludeId = null);
    Task<bool> HasAnyLoansAsync(int bookId);
}
