using LibrarySystem.Data.Entities;

namespace LibrarySystem.Data.Interfaces;

/// <summary>
/// Defines the contract for Book data access operations.
/// </summary>
public interface IBookRepository
{
    /// <summary>
    /// Retrieves a book by its unique identifier.
    /// </summary>
    /// <param name="id">The book's unique identifier.</param>
    /// <returns>The book if found; otherwise, null.</returns>
    Task<Book?> GetByIdAsync(int id);

    /// <summary>
    /// Retrieves all books from the catalogue.
    /// </summary>
    /// <returns>A read-only list of all books.</returns>
    Task<IReadOnlyList<Book>> GetAllAsync();

    /// <summary>
    /// Adds a new book to the database.
    /// </summary>
    /// <param name="book">The book entity to add.</param>
    Task AddAsync(Book book);

    /// <summary>
    /// Updates an existing book in the database.
    /// </summary>
    /// <param name="book">The book entity with updated values.</param>
    void Update(Book book);

    /// <summary>
    /// Deletes a book from the database.
    /// </summary>
    /// <param name="book">The book entity to delete.</param>
    void Delete(Book book);

    /// <summary>
    /// Checks if an ISBN is unique in the catalogue.
    /// </summary>
    /// <param name="isbn">The ISBN to check.</param>
    /// <param name="excludeId">Optional book ID to exclude from the check (used for updates).</param>
    /// <returns>True if the ISBN is unique; otherwise, false.</returns>
    Task<bool> IsIsbnUniqueAsync(string isbn, int? excludeId = null);
}
