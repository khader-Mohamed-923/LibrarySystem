using Microsoft.AspNetCore.Mvc;
using LibrarySystem.Contracts;
using LibrarySystem.Contracts.Requests.Book;
using LibrarySystem.Contracts.Responses.Book;
using LibrarySystem.Services.Implementations;
using LibrarySystem.Services.Exceptions;

namespace LibrarySystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController(BookService bookService) : ControllerBase
{

    [HttpPost]
    public async Task<ActionResult<ApiResponse<BookResponseDto>>> Create([FromBody] CreateBookDto dto)
    {
        var createdBook = await bookService.CreateBookAsync(dto);
        return Created($"/api/books/{createdBook.Id}", 
            ApiResponse<BookResponseDto>.SuccessResult(createdBook, ""));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<BookResponseDto>>> Update(int id, [FromBody] UpdateBookDto dto)
    {
        var updatedBook = await bookService.UpdateBookAsync(id, dto);
        return Ok(ApiResponse<BookResponseDto>.SuccessResult(updatedBook, "Book updated successfully."));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        await bookService.DeleteBookAsync(id);
        return Ok(ApiResponse<object>.SuccessResult(null!, "Book deleted successfully."));
    }
}
