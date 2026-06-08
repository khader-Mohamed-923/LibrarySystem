using Microsoft.AspNetCore.Mvc;
using LibrarySystem.Contracts;
using LibrarySystem.Contracts.Requests.Book;
using LibrarySystem.Contracts.Responses;
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
        catch (ResourceNotFoundException ex)
        {
            return StatusCode(ex.HttpStatusCode, ErrorResponse.Create(ex.ErrorCode.GetCode(), ex.Message, ex.HttpStatusCode));
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
        catch (DuplicateResourceException ex)
        {
            return StatusCode(ex.HttpStatusCode, ErrorResponse.Create(ex.ErrorCode.GetCode(), ex.Message, ex.HttpStatusCode));
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Update(int id, [FromBody] UpdateBookDto dto)
    {
        try
        {
            await bookService.UpdateBookAsync(id, dto);
            return Ok(ApiResponse<object>.SuccessResult(null!, "Book updated successfully."));
        }
        catch (ResourceNotFoundException ex)
        {
            return StatusCode(ex.HttpStatusCode, ErrorResponse.Create(ex.ErrorCode.GetCode(), ex.Message, ex.HttpStatusCode));
        }
        catch (DuplicateResourceException ex)
        {
            return StatusCode(ex.HttpStatusCode, ErrorResponse.Create(ex.ErrorCode.GetCode(), ex.Message, ex.HttpStatusCode));
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
        catch (ResourceNotFoundException ex)
        {
            return StatusCode(ex.HttpStatusCode, ErrorResponse.Create(ex.ErrorCode.GetCode(), ex.Message, ex.HttpStatusCode));
        }
    }
}
