using Microsoft.Extensions.DependencyInjection;
using LibrarySystem.Services.Implementations;
using LibrarySystem.Services.Interfaces;

namespace LibrarySystem.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<BookService>();
        services.AddScoped<MemberService>();
        services.AddScoped<ILoanService, LoanService>();

        return services;
    }
}
