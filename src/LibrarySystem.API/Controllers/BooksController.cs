using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using LibrarySystem.Contracts;
using LibrarySystem.Contracts.Requests.Book;
using LibrarySystem.Contracts.Responses.Book;
using LibrarySystem.Services.Services.Interfaces; 

namespace LibrarySystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]

public class BooksController(IBookService bookService, IValidator<CreateBookDto> createBookValidator) : ControllerBase
{

    [HttpPost]
    public async Task<ActionResult<ApiResponse<BookResponseDto>>> Create([FromBody] CreateBookDto dto)
    {
        var validationResult = await createBookValidator.ValidateAsync(dto);

        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            return BadRequest(ApiResponse<BookResponseDto>.FailureResult("Validation failed.", errors));
        }

        var createdBook = await bookService.CreateBookAsync(dto);
        return Created($"/api/books/{createdBook.Id}", 
            ApiResponse<BookResponseDto>.SuccessResult(createdBook, "Book created successfully."));
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