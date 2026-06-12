using Microsoft.EntityFrameworkCore;
using Shouldly;
using LibrarySystem.Data.Context;
using LibrarySystem.Data.Entities;
using LibrarySystem.Data.Repositories;
using LibrarySystem.UnitTests.Contexts;

namespace LibrarySystem.UnitTests.Repositories;

public class BookRepositoryTests
{
    private static LibraryDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new TestLibraryDbContext(options);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllBooks()
    {
        await using var ctx = CreateInMemoryContext();
        ctx.Books.AddRange(
            new Book { Title = "Book A", Author = "Author", ISBN = "9780000000001", TotalCopies = 2, AvailableCopies = 2 },
            new Book { Title = "Book B", Author = "Author", ISBN = "9780000000002", TotalCopies = 1, AvailableCopies = 1 },
            new Book { Title = "Book C", Author = "Author", ISBN = "9780000000003", TotalCopies = 1, AvailableCopies = 0 }
        );
        await ctx.SaveChangesAsync();

        var repo = new BookRepository(ctx);

        var all = await repo.GetAllAsync();

        all.Count.ShouldBe(3);
    }

    [Fact]
    public async Task GetByIdAsync_WhenBookExists_ReturnsCorrectBook()
    {
        await using var ctx = CreateInMemoryContext();
        var book = new Book
        {
            Title = "Clean Code",
            Author = "Robert Martin",
            ISBN = "9780123456789",
            TotalCopies = 5,
            AvailableCopies = 5
        };
        ctx.Books.Add(book);
        await ctx.SaveChangesAsync();

        var repo = new BookRepository(ctx);

        var result = await repo.GetByIdAsync(book.Id);

        result.ShouldNotBeNull();
        result!.ISBN.ShouldBe("9780123456789");
        result.Title.ShouldBe("Clean Code");
    }

    [Fact]
    public async Task GetByIdAsync_WhenBookDoesNotExist_ReturnsNull()
    {
        await using var ctx = CreateInMemoryContext();
        var repo = new BookRepository(ctx);

        var result = await repo.GetByIdAsync(999);

        result.ShouldBeNull();
    }

    [Fact]
    public async Task IsIsbnUniqueAsync_WhenIsbnExists_ReturnsFalse()
    {
        await using var ctx = CreateInMemoryContext();
        ctx.Books.Add(new Book
        {
            Title = "DDIA", Author = "Kleppmann", ISBN = "9781449373320",
            TotalCopies = 2, AvailableCopies = 1
        });
        await ctx.SaveChangesAsync();

        var repo = new BookRepository(ctx);

        var isUnique = await repo.IsIsbnUniqueAsync("9781449373320");

        isUnique.ShouldBeFalse();
    }

    [Fact]
    public async Task IsIsbnUniqueAsync_WhenIsbnDoesNotExist_ReturnsTrue()
    {
        await using var ctx = CreateInMemoryContext();
        var repo = new BookRepository(ctx);

        var isUnique = await repo.IsIsbnUniqueAsync("9799999999999");

        isUnique.ShouldBeTrue();
    }

    [Fact]
    public async Task AddAsync_AddsBookToDatabase()
    {
        await using var ctx = CreateInMemoryContext();
        var repo = new BookRepository(ctx);

        var book = new Book
        {
            Title = "New Book",
            Author = "New Author",
            ISBN = "9780000000099",
            TotalCopies = 3,
            AvailableCopies = 3
        };

        await repo.AddAsync(book);
        await ctx.SaveChangesAsync();

        var saved = await ctx.Books.FirstOrDefaultAsync(b => b.ISBN == "9780000000099");
        saved.ShouldNotBeNull();
        saved!.Title.ShouldBe("New Book");
    }

    [Fact]
    public async Task HasAnyLoansAsync_WhenBookHasLoans_ReturnsTrue()
    {
        await using var ctx = CreateInMemoryContext();

        var member = new Member
        {
            FirstName = "Test",
            LastName = "Member",
            Email = "test@lib.com",
            Phone = "1234567890",
            MembershipDate = DateTime.UtcNow,
            OutstandingFine = 0
        };
        ctx.Members.Add(member);

        var book = new Book
        {
            Title = "DDIA", Author = "Kleppmann", ISBN = "9781449373320",
            TotalCopies = 2, AvailableCopies = 1
        };
        ctx.Books.Add(book);
        await ctx.SaveChangesAsync();

        ctx.Loans.Add(new Loan
        {
            MemberId = member.Id,
            BookId = book.Id,
            LoanDate = DateTime.UtcNow.AddDays(-10),
            DueDate = DateTime.UtcNow.AddDays(4),
            IsReturned = false
        });
        await ctx.SaveChangesAsync();

        var repo = new BookRepository(ctx);

        var hasLoans = await repo.HasAnyLoansAsync(book.Id);

        hasLoans.ShouldBeTrue();
    }

    [Fact]
    public async Task HasAnyLoansAsync_WhenBookHasNoLoans_ReturnsFalse()
    {
        await using var ctx = CreateInMemoryContext();

        var book = new Book
        {
            Title = "Lonely Book", Author = "Author", ISBN = "9780000000077",
            TotalCopies = 1, AvailableCopies = 1
        };
        ctx.Books.Add(book);
        await ctx.SaveChangesAsync();

        var repo = new BookRepository(ctx);

        var hasLoans = await repo.HasAnyLoansAsync(book.Id);

        hasLoans.ShouldBeFalse();
    }
}