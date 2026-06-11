using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LibrarySystem.Data.Entities;

namespace LibrarySystem.Data.Configurations;

public class LoanConfiguration : IEntityTypeConfiguration<Loan>
{
    public void Configure(EntityTypeBuilder<Loan> builder)
    {
        builder.Property(l => l.LoanDate)
            .IsRequired()
            .HasColumnType("datetime2");

        builder.Property(l => l.DueDate)
            .IsRequired()
            .HasColumnType("datetime2");

        builder.Property(l => l.ReturnedAt)
            .HasColumnType("datetime2");

        builder.Property(l => l.FineAmount)
            .HasColumnType("decimal(10,2)")
            .HasDefaultValue(0m);

        builder.HasOne(l => l.Book)
            .WithMany(b => b.Loans)
            .HasForeignKey(l => l.BookId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.Member)
            .WithMany(m => m.Loans)
            .HasForeignKey(l => l.MemberId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
