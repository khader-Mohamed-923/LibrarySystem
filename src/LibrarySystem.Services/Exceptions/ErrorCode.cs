namespace LibrarySystem.Services.Exceptions;

public enum ErrorCode
{
    DUPLICATE_ISBN,
    BOOK_NOT_FOUND,
    MEMBER_NOT_FOUND,
    DUPLICATE_EMAIL,
    BOOK_NOT_AVAILABLE,
    LOAN_LIMIT_EXCEEDED,
    MEMBERSHIP_EXPIRED,
    OUTSTANDING_FINE,
    ALREADY_RETURNED
}

public static class ErrorCodeExtensions
{
    public static string GetCode(this ErrorCode errorCode)
    {
        return errorCode.ToString();
    }

    public static string GetMessage(this ErrorCode errorCode)
    {
        return errorCode switch
        {
            ErrorCode.DUPLICATE_ISBN => "A book with the same ISBN already exists.",
            ErrorCode.BOOK_NOT_FOUND => "The requested book was not found.",
            ErrorCode.MEMBER_NOT_FOUND => "The requested member was not found.",
            ErrorCode.DUPLICATE_EMAIL => "A member with the same email already exists.",
            ErrorCode.BOOK_NOT_AVAILABLE => "The book is currently not available for loan.",
            ErrorCode.LOAN_LIMIT_EXCEEDED => "The member has reached the maximum number of active loans.",
            ErrorCode.MEMBERSHIP_EXPIRED => "The member's membership has expired.",
            ErrorCode.OUTSTANDING_FINE => "The member has outstanding fines and cannot borrow books.",
            ErrorCode.ALREADY_RETURNED => "The loan has already been returned.",
            _ => "An unexpected error occurred."
        };
    }

    public static int GetHttpStatusCode(this ErrorCode errorCode)
    {
        return errorCode switch
        {
            ErrorCode.DUPLICATE_ISBN => 409,
            ErrorCode.BOOK_NOT_FOUND => 404,
            ErrorCode.MEMBER_NOT_FOUND => 404,
            ErrorCode.DUPLICATE_EMAIL => 409,
            ErrorCode.BOOK_NOT_AVAILABLE => 400,
            ErrorCode.LOAN_LIMIT_EXCEEDED => 400,
            ErrorCode.MEMBERSHIP_EXPIRED => 400,
            ErrorCode.OUTSTANDING_FINE => 400,
            ErrorCode.ALREADY_RETURNED => 400,
            _ => 500
        };
    }
}
