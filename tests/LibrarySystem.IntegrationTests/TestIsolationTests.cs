using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using LibrarySystem.Data;
using LibrarySystem.Data.Context;

namespace LibrarySystem.IntegrationTests;


public class TestIsolationTests : IClassFixture<LibraryWebAppFactory>, IAsyncLifetime
{
    private readonly LibraryWebAppFactory _factory;
    private readonly HttpClient _client;

    public TestIsolationTests(LibraryWebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public Task InitializeAsync() => TruncateLoansAsync();
    public Task DisposeAsync() => Task.CompletedTask;

   

    [Fact]
    public async Task IsolationTest_A_BorrowOneBook_ActiveLoanCountIsOne()
    {
        var (bookId, memberId) = await GetAvailableBookAndMember();

        await _client.PostAsJsonAsync("/api/loans", new { memberId, bookId });

        var loansResp = await _client.GetAsync($"/api/loans?memberId={memberId}");
        loansResp.EnsureSuccessStatusCode();
        var wrapper = await loansResp.Content.ReadFromJsonAsync<ApiResponse<List<LoanDto>>>();

   
        wrapper!.Data!.Count(l => l.ReturnedAt == null).ShouldBe(1,
            "Test A: exactly 1 active loan — isolation must be working");
    }

    [Fact]
    public async Task IsolationTest_B_BorrowOneBook_ActiveLoanCountIsOne()
    {
        
        var (bookId, memberId) = await GetAvailableBookAndMember();

        await _client.PostAsJsonAsync("/api/loans", new { memberId, bookId });

        var loansResp = await _client.GetAsync($"/api/loans?memberId={memberId}");
        loansResp.EnsureSuccessStatusCode();
        var wrapper = await loansResp.Content.ReadFromJsonAsync<ApiResponse<List<LoanDto>>>();

        wrapper!.Data!.Count(l => l.ReturnedAt == null).ShouldBe(1,
            "Test B: exactly 1 active loan — isolation must be working");
    }


    private async Task<(int bookId, int memberId)> GetAvailableBookAndMember()
    {
        var booksResp = await _client.GetAsync("/api/books");
        booksResp.EnsureSuccessStatusCode();
        var booksWrapper = await booksResp.Content.ReadFromJsonAsync<ApiResponse<List<BookDto>>>();

        var membersResp = await _client.GetAsync("/api/members");
        membersResp.EnsureSuccessStatusCode();
        var membersWrapper = await membersResp.Content.ReadFromJsonAsync<ApiResponse<List<MemberDto>>>();

        var book = booksWrapper!.Data!.First(b => b.AvailableCopies > 0 && b.Title != "Fully Booked");
        var member = membersWrapper!.Data!.First(m => m.MembershipDate > DateTime.UtcNow && m.OutstandingFine == 0);

        return (book.Id, member.Id);
    }

    private async Task TruncateLoansAsync()
    {
        await using var scope = _factory.CreateDbScope();
        var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();

        db.Loans.RemoveRange(db.Loans);

        var books = db.Books.ToList();
        foreach (var book in books)
            book.AvailableCopies = book.TotalCopies;

        await db.SaveChangesAsync();
    }


    private record ApiResponse<T>(bool Success, T? Data, string? Message);
    private record BookDto(int Id, string Title, int AvailableCopies);
    private record LoanDto(int Id, DateTime? ReturnedAt);
    private record MemberDto(int Id, DateTime MembershipDate, decimal OutstandingFine);
}