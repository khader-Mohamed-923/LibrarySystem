using LibrarySystem.Contracts.Requests.Book;
using LibrarySystem.Contracts.Responses.Book;
using LibrarySystem.Data.Entities;
using LibrarySystem.Data.Interfaces;
using LibrarySystem.Services.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Services.Implementations;

public class BookService
{
    private readonly IBookRepository _bookRepository;
    private readonly IUnitOfWork _unitOfWork;

    public BookService(IBookRepository bookRepository, IUnitOfWork unitOfWork)
    {
        _bookRepository = bookRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BookResponseDto> CreateBookAsync(CreateBookDto dto)
    {
        var isUnique = await _bookRepository.IsIsbnUniqueAsync(dto.Isbn);
        if (!isUnique)
        {
            throw new DuplicateResourceException(ErrorCode.DUPLICATE_ISBN);
        }

        var book = new Book
        {
            Title = dto.Title,
            Author = dto.Author,
            ISBN = dto.Isbn,
            TotalCopies = 1,
            AvailableCopies = 1
        };

        await _bookRepository.AddAsync(book);
        await _unitOfWork.SaveChangesAsync();
        return MapToDto(book);
    }

    public async Task<BookResponseDto> GetBookByIdAsync(int id)
    {
        var book = await _bookRepository.GetByIdAsync(id);
        if (book is null)
        {
            throw new ResourceNotFoundException(ErrorCode.BOOK_NOT_FOUND);
        }

        return MapToDto(book);
    }

    public async Task<IReadOnlyList<BookResponseDto>> GetAllBooksAsync()
    {
        var books = await _bookRepository.GetAllAsync();
        return books.Select(MapToDto).ToList();
    }

    public async Task<BookResponseDto> UpdateBookAsync(int id, UpdateBookDto dto)
    {
        var book = await _bookRepository.GetByIdAsync(id);
        if (book is null)
        {
            throw new ResourceNotFoundException(ErrorCode.BOOK_NOT_FOUND);
        }

        if (!string.IsNullOrWhiteSpace(dto.Isbn))
        {
            var isUnique = await _bookRepository.IsIsbnUniqueAsync(dto.Isbn, id);
            if (!isUnique)
            {
                throw new DuplicateResourceException(ErrorCode.DUPLICATE_ISBN);
            }
            book.ISBN = dto.Isbn;
        }

        if (!string.IsNullOrWhiteSpace(dto.Title))
        {
            book.Title = dto.Title;
        }

        if (!string.IsNullOrWhiteSpace(dto.Author))
        {
            book.Author = dto.Author;
        }

        _bookRepository.Update(book, dto.RowVersion);

        try
        {
            await _unitOfWork.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConcurrencyException(ErrorCode.CONCURRENCY_CONFLICT);
        }
        catch (DbUpdateException)
        {
            throw;
        }

        return MapToDto(book);
    }

    public async Task DeleteBookAsync(int id)
    {
        var book = await _bookRepository.GetByIdAsync(id);
        if (book is null)
        {
            throw new ResourceNotFoundException(ErrorCode.BOOK_NOT_FOUND);
        }

        _bookRepository.Delete(book);
        await _unitOfWork.SaveChangesAsync();
    }

    private static BookResponseDto MapToDto(Book book)
    {
        return new BookResponseDto
        {
            Id = book.Id,
            Title = book.Title,
            Author = book.Author,
            Isbn = book.ISBN,
            TotalCopies = book.TotalCopies,
            AvailableCopies = book.AvailableCopies,
            RowVersion = book.RowVersion
        };
    }
}
