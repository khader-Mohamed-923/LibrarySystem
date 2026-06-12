using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using LibrarySystem.Data.Context;
using LibrarySystem.Data.Entities;
using LibrarySystem.UnitTests.Contexts;

namespace LibrarySystem.UnitTests.Repositories;

public class MemberRepositoryConstraintTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly LibraryDbContext _ctx;

    public MemberRepositoryConstraintTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseSqlite(_connection)
            .Options;

        _ctx = new TestLibraryDbContext(options);
        _ctx.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _ctx.Dispose();
        _connection.Dispose();
    }

    private static Member MakeMember(string email) => new()
    {
        FirstName = "Test",
        LastName = "Member",
        Email = email,
        Phone = "1234567890",
        MembershipDate = DateTime.UtcNow,
        OutstandingFine = 0
    };

    private static Book MakeBook(string isbn) => new()
    {
        Title = "Test Book",
        Author = "Author",
        ISBN = isbn,
        TotalCopies = 1,
        AvailableCopies = 1
    };

    [Fact]
    public async Task InsertMember_DuplicateEmail_ThrowsDbUpdateException()
    {
        _ctx.Members.Add(MakeMember("duplicate@test.com"));
        await _ctx.SaveChangesAsync();

        _ctx.Members.Add(MakeMember("duplicate@test.com"));

        await Should.ThrowAsync<DbUpdateException>(() => _ctx.SaveChangesAsync());
    }

    [Fact]
    public async Task InsertBook_DuplicateISBN_ThrowsDbUpdateException()
    {
        _ctx.Books.Add(MakeBook("9781234567890"));
        await _ctx.SaveChangesAsync();

        _ctx.Books.Add(MakeBook("9781234567890"));

        await Should.ThrowAsync<DbUpdateException>(() => _ctx.SaveChangesAsync());
    }

    [Fact]
    public async Task DeleteMember_WithActiveLoans_ThrowsReferentialIntegrityException()
    {
        var member = MakeMember("linked@test.com");
        var book = MakeBook("9780000000099");

        _ctx.Members.Add(member);
        _ctx.Books.Add(book);
        await _ctx.SaveChangesAsync();

        _ctx.Loans.Add(new Loan
        {
            MemberId = member.Id,
            BookId = book.Id,
            LoanDate = DateTime.UtcNow.AddDays(-3),
            DueDate = DateTime.UtcNow.AddDays(11),
            IsReturned = false
        });
        await _ctx.SaveChangesAsync();

        _ctx.Members.Remove(member);

        await Should.ThrowAsync<Exception>(() => _ctx.SaveChangesAsync());
    }
}