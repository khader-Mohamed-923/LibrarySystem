namespace LibrarySystem.Contracts.Responses;

public record ErrorResponse
{
    public string Code { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public int HttpStatusCode { get; init; }
    public DateTime Timestamp { get; init; }

    public ErrorResponse(string code, string message, int httpStatusCode)
    {
        Code = code;
        Message = message;
        HttpStatusCode = httpStatusCode;
        Timestamp = DateTime.UtcNow;
    }

    public static ErrorResponse Create(string code, string message, int httpStatusCode)
    {
        return new ErrorResponse(code, message, httpStatusCode);
    }
}
