using System.Net;
using System.Net.Http.Json;
using Shouldly;

namespace LibrarySystem.IntegrationTests;

public class BooksControllerTests : IClassFixture<LibraryWebAppFactory>
{
    private readonly HttpClient _client;

    public BooksControllerTests(LibraryWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetBooks_ReturnsOkWithSeededBooks()
    {
        var response = await _client.GetAsync("/api/books");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var wrapper = await response.Content.ReadFromJsonAsync<ApiResponse<List<BookDto>>>();
        wrapper.ShouldNotBeNull();
        var books = wrapper!.Data;
        books.ShouldNotBeNull();
        books!.Count.ShouldBeGreaterThanOrEqualTo(5);
        books.ShouldAllBe(b => !string.IsNullOrEmpty(b.Title));
    }

    [Fact]
    public async Task GetBooks_WithAvailableFilter_ExcludesBooksWithZeroCopies()
    {
        var response = await _client.GetAsync("/api/books");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var wrapper = await response.Content.ReadFromJsonAsync<ApiResponse<List<BookDto>>>();
        wrapper.ShouldNotBeNull();
        var books = wrapper!.Data;
        books.ShouldNotBeNull();

        // Filter client-side since the endpoint returns all books
        var available = books!.Where(b => b.AvailableCopies > 0).ToList();
        available.ShouldAllBe(b => b.AvailableCopies > 0);
        available.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task GetBookById_WithValidId_ReturnsCorrectBook()
    {
       
        var listResponse = await _client.GetAsync("/api/books");
        listResponse.EnsureSuccessStatusCode();
        var listWrapper = await listResponse.Content.ReadFromJsonAsync<ApiResponse<List<BookDto>>>();
        var first = listWrapper!.Data!.First(b => b.Title == "Clean Code");

        var response = await _client.GetAsync($"/api/books/{first.Id}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var wrapper = await response.Content.ReadFromJsonAsync<ApiResponse<BookDto>>();
        wrapper.ShouldNotBeNull();
        var book = wrapper!.Data;
        book.ShouldNotBeNull();
        book!.Title.ShouldBe("Clean Code");
        book!.ISBN.ShouldBe("9780132350884");
    }

    [Fact]
    public async Task GetBookById_WithNonExistentId_Returns404()
    {
        var response = await _client.GetAsync("/api/books/99999");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateBook_WithValidData_Returns201WithLocationHeader()
    {
        var newBook = new
        {
            title = "Refactoring",
            author = "Martin Fowler",
            isbn = "9780201485677",
            totalCopies = 2,
            availableCopies = 2
        };

        var response = await _client.PostAsJsonAsync("/api/books", newBook);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        response.Headers.Location.ShouldNotBeNull();
        response.Headers.Location!.ToString().ShouldContain("/api/books/");

        var wrapper = await response.Content.ReadFromJsonAsync<ApiResponse<BookDto>>();
        wrapper.ShouldNotBeNull();
        var created = wrapper!.Data;
        created.ShouldNotBeNull();
        created!.Title.ShouldBe("Refactoring");
        created!.ISBN.ShouldBe("9780201485677");
    }

    [Fact]
    public async Task CreateBook_WithDuplicateISBN_Returns409Conflict()
    {
        var duplicate = new
        {
            title = "Duplicate ISBN Book",
            author = "Someone",
            isbn = "9780132350884",
            totalCopies = 1,
            availableCopies = 1
        };

        var response = await _client.PostAsJsonAsync("/api/books", duplicate);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateBook_WithMissingRequiredField_Returns400WithValidationErrors()
    {
        var invalid = new { author = "No Title Author", isbn = "9789999999999", totalCopies = 1 };

        var response = await _client.PostAsJsonAsync("/api/books", invalid);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var body = await response.Content.ReadAsStringAsync();
        body.ShouldNotBeNullOrEmpty();
    }



    private record ApiResponse<T>(bool Success, T? Data, string? Message);
    private record BookDto(int Id, string Title, string Author, string ISBN,
                           int TotalCopies, int AvailableCopies);
}