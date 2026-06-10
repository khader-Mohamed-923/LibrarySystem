using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LibrarySystem.Data.Entities;

namespace LibrarySystem.Data.Configurations;

public class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.Property(m => m.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasIndex(m => m.Email)
            .HasDatabaseName("UX_Members_Email")
            .IsUnique();

        builder.Property(m => m.Phone)
            .IsRequired()
            .HasMaxLength(20);



        builder.Property(m => m.RowVersion)
            .IsRowVersion();

        builder.HasMany(m => m.Loans)
            .WithOne(l=>l.Member)
            .HasForeignKey(l=>l.MemberId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}