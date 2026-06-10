namespace LibrarySystem.Services.Exceptions;

public class ConcurrencyException : LibraryException
{
    public ConcurrencyException(ErrorCode errorCode)
        : base(errorCode)
    {
    }

    public ConcurrencyException(ErrorCode errorCode, string message)
        : base(errorCode, message)
    {
    }
}
