using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using LibrarySystem.Data.Context;
using LibrarySystem.Data.Repositories;
using LibrarySystem.Data.Interfaces;

namespace LibrarySystem.Data;

public static class DependencyInjection
{
    public static IServiceCollection AddDataInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<LibraryDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IBookRepository, BookRepository>();
        services.AddScoped<IMemberRepository, MemberRepository>();
        services.AddScoped<ILoanRepository, LoanRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
