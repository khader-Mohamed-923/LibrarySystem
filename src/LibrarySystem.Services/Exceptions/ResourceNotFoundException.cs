namespace LibrarySystem.Services.Exceptions;

public class ResourceNotFoundException : LibraryException
{
    public ResourceNotFoundException(ErrorCode errorCode)
        : base(errorCode)
    {
    }

    public ResourceNotFoundException(ErrorCode errorCode, string message)
        : base(errorCode, message)
    {
    }
}
