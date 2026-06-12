using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LibrarySystem.Data.Context;
using LibrarySystem.Data.Entities;

namespace LibrarySystem.UnitTests.Contexts;


public class TestLibraryDbContext : LibraryDbContext
{
    public TestLibraryDbContext(DbContextOptions<LibraryDbContext> options)
        : base(options)
    {
       
        ChangeTracker.CascadeDeleteTiming = Microsoft.EntityFrameworkCore.ChangeTracking.CascadeTiming.OnSaveChanges;
        ChangeTracker.DeleteOrphansTiming = Microsoft.EntityFrameworkCore.ChangeTracking.CascadeTiming.OnSaveChanges;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.EnableSensitiveDataLogging();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

      
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var rowVersionProperty = entityType.FindProperty("RowVersion");
            if (rowVersionProperty != null) rowVersionProperty.IsNullable = true;
        }


        modelBuilder.Entity<Loan>()
            .HasOne(l => l.Member)
            .WithMany(m => m.Loans)
            .HasForeignKey(l => l.MemberId)
            .OnDelete(DeleteBehavior.ClientNoAction);
    }

    public override int SaveChanges()
    {
        SetRowVersions();
        DetachOrphanedLoans();
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        SetRowVersions();
        DetachOrphanedLoans();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetRowVersions();
        DetachOrphanedLoans();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        SetRowVersions();
        DetachOrphanedLoans();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void DetachOrphanedLoans()
    {
        // Find all deleted members
        var deletedMembers = ChangeTracker.Entries<Member>()
            .Where(e => e.State == EntityState.Deleted)
            .Select(e => e.Entity.Id)
            .ToList();

        if (!deletedMembers.Any()) return;

        // Detach any tracked loans that belong to these deleted members
        var orphanedLoans = ChangeTracker.Entries<Loan>()
            .Where(e => deletedMembers.Contains(e.Entity.MemberId))
            .ToList();

        foreach (var loanEntry in orphanedLoans)
        {
            loanEntry.State = EntityState.Detached;
        }
    }

    private void SetRowVersions()
    {
        // Find all added or modified entities in the change tracker
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        var defaultRowVersion = new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 };

        foreach (var entry in entries)
        {
            // Look for a RowVersion property (often configured as a concurrency token/timestamp)
            var rowVersionProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "RowVersion");

            // If the property exists and is empty/null, assign a dummy value
            if (rowVersionProperty != null)
            {
                var currentValue = rowVersionProperty.CurrentValue as byte[];
                if (currentValue == null || currentValue.Length == 0)
                {
                    rowVersionProperty.CurrentValue = defaultRowVersion;
                }
            }
        }
    }
}
