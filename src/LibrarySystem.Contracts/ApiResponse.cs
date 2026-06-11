namespace LibrarySystem.Contracts;

public record ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public string? Message { get; init; }
    public Dictionary<string, string[]>? Errors { get; init; } 
    public DateTime CreatedAt { get; init; }

  
    private ApiResponse(bool success, T? data, string? message, Dictionary<string, string[]>? errors = null)
    {
        Success = success;
        Data = data;
        Message = message;
        Errors = errors;
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

   
    public static ApiResponse<T> FailureResult(string? message, Dictionary<string, string[]>? errors)
    {
        return new ApiResponse<T>(false, default, message == null ? "Validation failed" : message, errors);
    }
}