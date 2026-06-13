using LibrarySystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LibrarySystem.Data.Context;

public static class DataSeeder
{
    public static async Task SeedDatabaseAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();


        await context.Database.MigrateAsync();


        var currentBookCount = await context.Books.CountAsync();
        if (currentBookCount < 50)
        {
            var booksNeeded = 50 - currentBookCount;
            var books = new List<Book>();

            for (int i = 1; i <= booksNeeded; i++)
            {
                var bookIndex = currentBookCount + i;
                books.Add(new Book
                {
                    Title = $"Load Test Book {bookIndex}",
                    Author = $"Test Author {bookIndex}",
                    ISBN = $"ISBN{bookIndex}",
                    TotalCopies = 5000,
                    AvailableCopies = 5000
                });
            }

            await context.Books.AddRangeAsync(books);
            await context.SaveChangesAsync();
        }


        var currentMemberCount = await context.Members.CountAsync();
        if (currentMemberCount < 200)
        {
            var membersNeeded = 200 - currentMemberCount;
            var members = new List<Member>();

            for (int i = 1; i <= membersNeeded; i++)
            {
                var memberIndex = currentMemberCount + i;
                members.Add(new Member
                {
                    FirstName = $"TestUser",
                    LastName = $"{memberIndex}",
                    Email = $"testuser{memberIndex}@library.local",
                    Phone = $"555-0199-{memberIndex:D3}",
                    MembershipDate = DateTime.UtcNow,
                    OutstandingFine = 0
                });
            }

            await context.Members.AddRangeAsync(members);
            await context.SaveChangesAsync();
        }
    }
}
