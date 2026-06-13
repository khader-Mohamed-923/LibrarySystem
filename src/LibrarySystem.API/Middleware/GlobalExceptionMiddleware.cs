using System.Text.Json;
using LibrarySystem.Contracts.Responses;
using LibrarySystem.Services.Exceptions;

namespace LibrarySystem.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

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
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, errorCode, errorType) = exception switch
        {
            BusinessRuleViolationException brEx =>
                (brEx.HttpStatusCode, brEx.ErrorCode.GetCode(), "BusinessRuleViolation"),

            ResourceNotFoundException rnfEx =>
                (rnfEx.HttpStatusCode, rnfEx.ErrorCode.GetCode(), "ResourceNotFound"),

            DuplicateResourceException drEx =>
                (drEx.HttpStatusCode, drEx.ErrorCode.GetCode(), "DuplicateResource"),

            ConcurrencyException cEx =>
                (cEx.HttpStatusCode, cEx.ErrorCode.GetCode(), "ConcurrencyConflict"),

           
            LibraryException libEx =>
                (libEx.HttpStatusCode, libEx.ErrorCode.GetCode(), "DomainError"),

            _ =>
                (StatusCodes.Status500InternalServerError, "INTERNAL_SERVER_ERROR", "UnhandledError")
        };

      
        if (exception is LibraryException)
            _logger.LogWarning(exception, "Domain exception [{ErrorType}]: {Message}", errorType, exception.Message);
        else
            _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = ErrorResponse.Create(errorCode, exception.Message, statusCode);
        var json = JsonSerializer.Serialize(response, JsonOptions);

        await context.Response.WriteAsync(json);
    }
}
