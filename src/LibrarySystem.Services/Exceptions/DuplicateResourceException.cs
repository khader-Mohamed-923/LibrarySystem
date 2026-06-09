namespace LibrarySystem.Services.Exceptions;

public class DuplicateResourceException : LibraryException
{
    public DuplicateResourceException(ErrorCode errorCode)
        : base(errorCode)
    {
    }

    public DuplicateResourceException(ErrorCode errorCode, string message)
        : base(errorCode, message)
    {
    }
}
