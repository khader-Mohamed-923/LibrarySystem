using Microsoft.Extensions.DependencyInjection;
using LibrarySystem.Services.Services.Implementations;
using LibrarySystem.Services.Services.Interfaces;

namespace LibrarySystem.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IBookService, BookService>();
        services.AddScoped<IMemberService, MemberService>();
        services.AddScoped<ILoanService, LoanService>();

        services.AddSingleton(TimeProvider.System);

        return services;
    }
}
