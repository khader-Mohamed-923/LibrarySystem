using Microsoft.Extensions.DependencyInjection;
using LibrarySystem.Services.Implementations;

namespace LibrarySystem.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<BookService>();
        services.AddScoped<MemberService>();

        return services;
    }
}
