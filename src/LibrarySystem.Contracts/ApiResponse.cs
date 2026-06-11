namespace LibrarySystem.Contracts;

public record ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public string? Message { get; init; }
    public DateTime CreatedAt { get; init; }

    private ApiResponse(bool success, T? data, string? message)
    {
        Success = success;
        Data = data;
        Message = message;
        CreatedAt = DateTime.UtcNow;
    }

    public static ApiResponse<T> SuccessResult(T data, string? message)
    {
        return new ApiResponse<T>(true, data, message == null ? "operation success" : message);
    }

    public static ApiResponse<T> FailureResult(string? message)
    {
        return new ApiResponse<T>(false, default, message == null ? "operation Failure" : message);
    }
}
