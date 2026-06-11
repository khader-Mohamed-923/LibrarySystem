using FluentValidation;
using LibrarySystem.Services.Services.Implementations; 

namespace LibrarySystem.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddControllers();

        services.AddValidatorsFromAssembly(typeof(LoanService).Assembly); 

        services.AddOpenApi();
        return services;
    }
}