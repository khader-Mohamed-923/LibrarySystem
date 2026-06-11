using LibrarySystem.Contracts.Requests.Book;
using LibrarySystem.Contracts.Responses.Book;

namespace LibrarySystem.Services.Services.Interfaces;

public interface IBookService
{
    Task<BookResponseDto> CreateBookAsync(CreateBookDto dto);
    Task<BookResponseDto> GetBookByIdAsync(int id);
    Task<IReadOnlyList<BookResponseDto>> GetAllBooksAsync();
    Task<BookResponseDto> UpdateBookAsync(int id, UpdateBookDto dto);
    Task DeleteBookAsync(int id);
}