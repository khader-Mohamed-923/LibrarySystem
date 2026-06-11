using System.Net;
using System.Text.Json;
using LibrarySystem.Contracts.Responses;
using LibrarySystem.Services.Exceptions;


namespace LibrarySystem.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (LibraryException ex)
        {
            _logger.LogWarning(ex, "A domain exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex, ex.HttpStatusCode, ex.ErrorCode.GetCode());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred.");
            await HandleExceptionAsync(context, ex, (int)HttpStatusCode.InternalServerError, "INTERNAL_SERVER_ERROR");
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception, int statusCode, string errorCode)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = ErrorResponse.Create(errorCode, exception.Message, statusCode);
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        return context.Response.WriteAsync(json);
    }
}
