using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LibrarySystem.Data.Entities;

namespace LibrarySystem.Data.Configurations;

public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.Property(b => b.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.Author)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(b => b.ISBN)
            .IsRequired()
            .IsFixedLength()
            .HasMaxLength(13);

        builder.HasIndex(b => b.ISBN)
            .HasDatabaseName("UX_Books_ISBN")
            .IsUnique();


        builder.Property(b => b.RowVersion)
            .IsRowVersion();

        builder.ToTable(b => b.HasCheckConstraint(
            "CK_Books_TotalCopies",
            "[TotalCopies] >= 1"));

        builder.ToTable(b => b.HasCheckConstraint(
            "CK_Books_AvailableCopies",
            "[AvailableCopies] >= 0"));

        builder.HasIndex(b => b.AvailableCopies)
            .HasDatabaseName("IX_Books_AvailableCopies")
            .HasFilter("[AvailableCopies] > 0");

        builder.HasMany(b => b.Loans)
            .WithOne(l => l.Book)
            .HasForeignKey(l => l.BookId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}