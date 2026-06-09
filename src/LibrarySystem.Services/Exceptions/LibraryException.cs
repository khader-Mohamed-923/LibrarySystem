namespace LibrarySystem.Services.Exceptions;

public abstract class LibraryException : Exception
{
    public ErrorCode ErrorCode { get; }
    public int HttpStatusCode { get; }

    protected LibraryException(ErrorCode errorCode)
        : base(errorCode.GetMessage())
    {
        ErrorCode = errorCode;
        HttpStatusCode = errorCode.GetHttpStatusCode();
    }

    protected LibraryException(ErrorCode errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
        HttpStatusCode = errorCode.GetHttpStatusCode();
    }
}
