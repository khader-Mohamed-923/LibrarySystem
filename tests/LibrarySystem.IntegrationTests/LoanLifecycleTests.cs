using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using LibrarySystem.Data;
using LibrarySystem.Data.Context;

namespace LibrarySystem.IntegrationTests;

public class LoanLifecycleTests : IClassFixture<LibraryWebAppFactory>, IAsyncLifetime
{
    private readonly LibraryWebAppFactory _factory;
    private readonly HttpClient _client;

    public LoanLifecycleTests(LibraryWebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public Task InitializeAsync() => TruncateLoansAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task BorrowBook_ValidRequest_Returns201WithCorrectDueDate()
    {
        var (bookId, memberId) = await GetAvailableBookAndMember();

        var response = await _client.PostAsJsonAsync("/api/loans", new { memberId, bookId });

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var wrapper = await response.Content.ReadFromJsonAsync<ApiResponse<LoanDto>>();
        wrapper.ShouldNotBeNull();
        var loan = wrapper!.Data;
        loan.ShouldNotBeNull();
        loan!.ReturnedAt.ShouldBeNull();
        
        var expectedDue = DateTime.UtcNow.AddDays(14);
        loan.DueDate.ShouldBeInRange(expectedDue.AddSeconds(-5), expectedDue.AddSeconds(5));
    }

    [Fact]
    public async Task BorrowBook_SuccessfulBorrow_DecrementsAvailableCopies()
    {
        var (bookId, memberId) = await GetAvailableBookAndMember();
        var before = await GetBookById(bookId);
        var copiesBefore = before.AvailableCopies;

        await _client.PostAsJsonAsync("/api/loans", new { memberId, bookId });

        var after = await GetBookById(bookId);
        after.AvailableCopies.ShouldBe(copiesBefore - 1);
    }

    [Fact]
    public async Task BorrowBook_WhenNoCopiesAvailable_Returns422WithMessage()
    {
        // Arrange
        var (bookId, memberId) = await GetAvailableBookAndMember();

        await using var scope = _factory.CreateDbScope();
        var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
        var book = await db.Books.FindAsync(bookId);
        book!.AvailableCopies = 0;
        await db.SaveChangesAsync();

        // Act
        var response = await _client.PostAsJsonAsync("/api/loans", new { memberId, bookId });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        var body = await response.Content.ReadAsStringAsync();
        body.ToLower().ShouldContain("available");
    }

    [Fact]
    public async Task BorrowBook_WhenMemberHas3ActiveLoans_Returns422WithLoanLimitMessage()
    {
        var member = await GetMemberWithNoLoans();
        var availableBooks = await GetThreeDifferentBooks();

        foreach (var bookId in availableBooks)
            await _client.PostAsJsonAsync("/api/loans", new { memberId = member, bookId });

        var oneMoreBook = await GetAnotherAvailableBook(availableBooks);

        var response = await _client.PostAsJsonAsync("/api/loans", new { memberId = member, bookId = oneMoreBook });

        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        var body = await response.Content.ReadAsStringAsync();
        body.ToLower().ShouldContain("limit");
    }

    [Fact]
    public async Task ReturnBook_ValidActiveLoan_Returns200WithReturnedAtAndZeroFine()
    {
        var (bookId, memberId) = await GetAvailableBookAndMember();
        var borrowResp = await _client.PostAsJsonAsync("/api/loans", new { memberId, bookId });
        borrowResp.EnsureSuccessStatusCode();
        var borrowWrapper = await borrowResp.Content.ReadFromJsonAsync<ApiResponse<LoanDto>>();
        var loan = borrowWrapper!.Data;

        var returnResp = await _client.PutAsync($"/api/loans/{loan!.Id}/return", null);

        returnResp.StatusCode.ShouldBe(HttpStatusCode.OK);
        var returnWrapper = await returnResp.Content.ReadFromJsonAsync<ApiResponse<LoanDto>>();
        var returned = returnWrapper!.Data;
        returned.ShouldNotBeNull();
        returned!.ReturnedAt.ShouldNotBeNull();
        returned.FineAmount.ShouldBe(0m);
    }

    [Fact]
    public async Task ReturnBook_AfterReturn_IncrementsAvailableCopies()
    {
        var (bookId, memberId) = await GetAvailableBookAndMember();
        await _client.PostAsJsonAsync("/api/loans", new { memberId, bookId });
        var afterBorrow = await GetBookById(bookId);
        var copiesAfterBorrow = afterBorrow.AvailableCopies;

        var loansResp = await _client.GetAsync($"/api/loans?memberId={memberId}");
        loansResp.EnsureSuccessStatusCode();
        var loansWrapper = await loansResp.Content.ReadFromJsonAsync<ApiResponse<List<LoanDto>>>();
        var active = loansWrapper!.Data!.First(l => l.ReturnedAt == null);

        await _client.PutAsync($"/api/loans/{active.Id}/return", null);

        var afterReturn = await GetBookById(bookId);
        afterReturn.AvailableCopies.ShouldBe(copiesAfterBorrow + 1);
    }

    [Fact]
    public async Task ReturnBook_AlreadyReturned_Returns422()
    {
        var (bookId, memberId) = await GetAvailableBookAndMember();
        var borrowResp = await _client.PostAsJsonAsync("/api/loans", new { memberId, bookId });
        borrowResp.EnsureSuccessStatusCode();
        var borrowWrapper = await borrowResp.Content.ReadFromJsonAsync<ApiResponse<LoanDto>>();
        var loan = borrowWrapper!.Data;
        await _client.PutAsync($"/api/loans/{loan!.Id}/return", null);

        var secondReturn = await _client.PutAsync($"/api/loans/{loan.Id}/return", null);

        secondReturn.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task FullLoanCycle_BorrowReturnBorrowAgainByDifferentMember_CopiesAreConsistent()
    {
        var books = await GetAllBooks();
        var members = await GetAllMembers();
        var book = books.First(b => b.AvailableCopies > 0 && b.Title != "Fully Booked");
        var member1 = members[0];
        var member2 = members[1];
        var initial = book.AvailableCopies;

        var borrow1 = await _client.PostAsJsonAsync("/api/loans", new { memberId = member1.Id, bookId = book.Id });
        borrow1.EnsureSuccessStatusCode();
        var borrow1Wrapper = await borrow1.Content.ReadFromJsonAsync<ApiResponse<LoanDto>>();
        var loan1 = borrow1Wrapper!.Data;
        await _client.PutAsync($"/api/loans/{loan1!.Id}/return", null);
        var borrow2 = await _client.PostAsJsonAsync("/api/loans", new { memberId = member2.Id, bookId = book.Id });
        borrow2.EnsureSuccessStatusCode();

        var finalBook = await GetBookById(book.Id);
        finalBook.AvailableCopies.ShouldBe(initial - 1);
    }

    // ── Helper methods ──────────────────────────────────────────────────

    private async Task<List<BookDto>> GetAllBooks()
    {
        var response = await _client.GetAsync("/api/books");
        response.EnsureSuccessStatusCode();
        var wrapper = await response.Content.ReadFromJsonAsync<ApiResponse<List<BookDto>>>();
        wrapper.ShouldNotBeNull();
        wrapper!.Data.ShouldNotBeNull();
        return wrapper.Data!;
    }

    private async Task<BookDto> GetBookById(int id)
    {
        var response = await _client.GetAsync($"/api/books/{id}");
        response.EnsureSuccessStatusCode();
        var wrapper = await response.Content.ReadFromJsonAsync<ApiResponse<BookDto>>();
        wrapper.ShouldNotBeNull();
        wrapper!.Data.ShouldNotBeNull();
        return wrapper.Data!;
    }

    private async Task<(int bookId, int memberId)> GetAvailableBookAndMember()
    {
        var books = await GetAllBooks();
        var members = await GetAllMembers();
        var book = books.First(b => b.AvailableCopies > 0 && b.Title != "Fully Booked");
        var member = members.First(m => m.MembershipDate > DateTime.UtcNow && m.OutstandingFine == 0);
        return (book.Id, member.Id);
    }

    private async Task<int> GetMemberWithNoLoans()
    {
        var members = await GetAllMembers();
        return members.First(m => m.MembershipDate > DateTime.UtcNow).Id;
    }

    private async Task<List<int>> GetThreeDifferentBooks()
    {
        var books = await GetAllBooks();
        return books.Where(b => b.AvailableCopies > 0 && b.Title != "Fully Booked").Take(3).Select(b => b.Id).ToList();
    }

    private async Task<int> GetAnotherAvailableBook(List<int> excludeIds)
    {
        var books = await GetAllBooks();
        var book = books.FirstOrDefault(b => b.AvailableCopies > 0 && !excludeIds.Contains(b.Id));
        
        if (book == null)
        {
            throw new InvalidOperationException(
                $"Test Data Setup Failure: Not enough distinct available books to satisfy the test condition. " +
                $"Attempted to find an available book not in the excluded list [{string.Join(", ", excludeIds)}], but none were found. " +
                "Ensure your seeded test database has enough unique books in stock.");
        }

        return book.Id;
    }

    private async Task<List<MemberDto>> GetAllMembers()
    {
        var response = await _client.GetAsync("/api/members");
        response.EnsureSuccessStatusCode();
        var wrapper = await response.Content.ReadFromJsonAsync<ApiResponse<List<MemberDto>>>();
        wrapper.ShouldNotBeNull();
        wrapper!.Data.ShouldNotBeNull();
        return wrapper.Data!;
    }

    private async Task TruncateLoansAsync()
    {
        await using var scope = _factory.CreateDbScope();
        var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
        
        // Remove all loans
        db.Loans.RemoveRange(db.Loans);
        
        // CRITICAL FIX: Reset available copies back to total copies!
        // Otherwise, test state drifts because deleted loans don't automatically restore book inventory.
        var books = db.Books.ToList();
        foreach (var book in books)
        {
            book.AvailableCopies = book.TotalCopies;
        }

        await db.SaveChangesAsync();
    }

    // ── DTOs matching the API response shape ────────────────────────────

    private record ApiResponse<T>(bool Success, T? Data, string? Message);
    private record BookDto(int Id, string Title, int AvailableCopies);
    private record LoanDto(int Id, DateTime DueDate, DateTime? ReturnedAt, decimal FineAmount);
    private record MemberDto(int Id, string Email, DateTime MembershipDate, decimal OutstandingFine);
}