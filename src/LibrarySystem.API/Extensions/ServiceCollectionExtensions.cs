namespace LibrarySystem.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        // Add API specific services, filters, validators here
        return services;
    }
}
