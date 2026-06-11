using LibrarySystem.API.Middleware;

namespace LibrarySystem.API.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseGlobalExceptionMiddleware(this WebApplication app)
    {
        app.UseMiddleware<GlobalExceptionMiddleware>();
        return app;
    }
}
