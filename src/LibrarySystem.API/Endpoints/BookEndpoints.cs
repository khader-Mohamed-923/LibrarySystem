using LibrarySystem.Contracts;
using LibrarySystem.Contracts.Responses.Book;
using LibrarySystem.Services.Implementations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LibrarySystem.API.Endpoints;

public static class BookEndpoints
{
    public static IEndpointRouteBuilder MapBookEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("api/books");

        group.MapGet("/", async (BookService bookService) =>
        {
            var books = await bookService.GetAllBooksAsync();
            return Results.Ok(ApiResponse<IReadOnlyList<BookResponseDto>>.SuccessResult(books, "All books fetched successfully."));
        });

        group.MapGet("/{id:int}", async (int id, BookService bookService) =>
        {
            var book = await bookService.GetBookByIdAsync(id);
            return Results.Ok(ApiResponse<BookResponseDto>.SuccessResult(book, "Book fetched successfully."));
        });

        return builder;
    }
}
