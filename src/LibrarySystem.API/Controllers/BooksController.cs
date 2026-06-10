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
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<BookResponseDto>>>> GetAll()
    {
        var books = await bookService.GetAllBooksAsync();
        return Ok(ApiResponse<IReadOnlyList<BookResponseDto>>.SuccessResult(books));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<BookResponseDto>>> GetById(int id)
    {
        try
        {
            var book = await bookService.GetBookByIdAsync(id);
            return Ok(ApiResponse<BookResponseDto>.SuccessResult(book));
        }
        catch (LibraryException ex)
        {
            return StatusCode(ex.HttpStatusCode, new { code = ex.ErrorCode.GetCode(), message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<BookResponseDto>>> Create([FromBody] CreateBookDto dto)
    {
        try
        {
            var createdBook = await bookService.CreateBookAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = createdBook.Id }, 
                ApiResponse<BookResponseDto>.SuccessResult(createdBook));
        }
        catch (LibraryException ex)
        {
            return StatusCode(ex.HttpStatusCode, new { code = ex.ErrorCode.GetCode(), message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<BookResponseDto>>> Update(int id, [FromBody] UpdateBookDto dto)
    {
        try
        {
            var updatedBook = await bookService.UpdateBookAsync(id, dto);
            return Ok(ApiResponse<BookResponseDto>.SuccessResult(updatedBook, "Book updated successfully."));
        }
        catch (LibraryException ex)
        {
            return StatusCode(ex.HttpStatusCode, new { code = ex.ErrorCode.GetCode(), message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        try
        {
            await bookService.DeleteBookAsync(id);
            return Ok(ApiResponse<object>.SuccessResult(null!, "Book deleted successfully."));
        }
        catch (LibraryException ex)
        {
            return StatusCode(ex.HttpStatusCode, new { code = ex.ErrorCode.GetCode(), message = ex.Message });
        }
    }
}
