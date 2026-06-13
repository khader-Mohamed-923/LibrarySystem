using LibrarySystem.API.Middleware;
using LibrarySystem.Data.Context; // ضيف الـ Namespace الخاص بالـ Seeder

namespace LibrarySystem.API.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseGlobalExceptionMiddleware(this WebApplication app)
    {
        app.UseMiddleware<GlobalExceptionMiddleware>();
        return app;
    }


    public static async Task SeedDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        await DataSeeder.SeedDatabaseAsync(scope.ServiceProvider);
    }
}