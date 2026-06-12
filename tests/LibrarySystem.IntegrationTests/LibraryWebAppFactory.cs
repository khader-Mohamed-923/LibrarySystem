using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;
using LibrarySystem.Data;
using LibrarySystem.Data.Context;
using LibrarySystem.Data.Entities;

namespace LibrarySystem.IntegrationTests;

public class LibraryWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _dbContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.MigrateAsync();

        await SeedData(db);
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await _dbContainer.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<LibraryDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<LibraryDbContext>(options =>
            {
                var connectionString = _dbContainer.GetConnectionString();
                var csBuilder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connectionString)
                {
                    InitialCatalog = "LibraryTestDb"
                };
                options.UseSqlServer(csBuilder.ConnectionString);
            });
        });
    }

    private static async Task SeedData(LibraryDbContext db)
    {
        var books = new List<Book>
        {
            new() { Title = "Clean Code", Author = "Robert Martin", ISBN = "9780132350884", TotalCopies = 3, AvailableCopies = 3 },
            new() { Title = "Domain-Driven Design", Author = "Eric Evans", ISBN = "9780321125217", TotalCopies = 2, AvailableCopies = 2 },
            new() { Title = "The Pragmatic Programmer", Author = "Hunt & Thomas", ISBN = "9780201616224", TotalCopies = 2, AvailableCopies = 2 },
            new() { Title = "DDIA", Author = "Kleppmann", ISBN = "9781449373320", TotalCopies = 1, AvailableCopies = 1 },
            new() { Title = "Design Patterns", Author = "Gang of Four", ISBN = "9780201633610", TotalCopies = 5, AvailableCopies = 5 },
            new() { Title = "Fully Booked", Author = "No Author", ISBN = "9780000000001", TotalCopies = 1, AvailableCopies = 0 },
        };

        var members = new List<Member>
        {
            new() { FirstName = "Alice", LastName = "Smith", Email = "alice@test.com", MembershipDate = DateTime.UtcNow.AddYears(1), OutstandingFine = 0 },
            new() { FirstName = "Bob", LastName = "Jones", Email = "bob@test.com", MembershipDate = DateTime.UtcNow.AddYears(1), OutstandingFine = 0 },
            new() { FirstName = "Carol", LastName = "White", Email = "carol@test.com", MembershipDate = DateTime.UtcNow.AddYears(1), OutstandingFine = 0 },
            new() { FirstName = "David", LastName = "Brown", Email = "david@test.com", MembershipDate = DateTime.UtcNow.AddYears(1), OutstandingFine = 0 },
            new() { FirstName = "Expired", LastName = "Member", Email = "expired@test.com", MembershipDate = DateTime.UtcNow.AddDays(-1), OutstandingFine = 0 },
        };

        db.Books.AddRange(books);
        db.Members.AddRange(members);
        await db.SaveChangesAsync();
    }

    public AsyncServiceScope CreateDbScope() => Services.CreateAsyncScope();
}