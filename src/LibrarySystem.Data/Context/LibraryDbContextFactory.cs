using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LibrarySystem.Data.Context;


public class LibraryDbContextFactory : IDesignTimeDbContextFactory<LibraryDbContext>
{
    private const string ConnectionStringEnvironmentVariable = "LibraryDb_ConnectionString";

   
    public LibraryDbContext CreateDbContext(string[] args)
    {
       
        var connectionString = Environment.GetEnvironmentVariable(ConnectionStringEnvironmentVariable);

    
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Environment variable '{ConnectionStringEnvironmentVariable}' is not set. " +
                "Please set it in your terminal session before running migrations.");
        }

      
        var optionsBuilder = new DbContextOptionsBuilder<LibraryDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new LibraryDbContext(optionsBuilder.Options);
    }
}
