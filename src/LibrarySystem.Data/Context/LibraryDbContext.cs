using Microsoft.EntityFrameworkCore;
using LibrarySystem.Data.Entities;

namespace LibrarySystem.Data.Context;

public class LibraryDbContext : DbContext
{
    public LibraryDbContext(DbContextOptions<LibraryDbContext> options)
        : base(options)
    {
    }

    public DbSet<Book> Books => Set<Book>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Loan> Loans => Set<Loan>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {


        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LibraryDbContext).Assembly);
    }
}

