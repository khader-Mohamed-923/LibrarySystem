namespace LibrarySystem.Services.Exceptions;

public static class ErrorCodeInfo
{
    public static string GetCode(this ErrorCode errorCode)
    {
        return errorCode.ToString();
    }

    public static string GetMessage(this ErrorCode errorCode)
    {
        return errorCode switch
        {
            ErrorCode.BOOK_NOT_FOUND => "The requested book was not found.",
            ErrorCode.DUPLICATE_ISBN => "A book with the same ISBN already exists.",
            ErrorCode.MEMBER_NOT_FOUND => "The requested member was not found.",
            ErrorCode.DUPLICATE_EMAIL => "A member with the same email already exists.",
            ErrorCode.MEMBERSHIP_EXPIRED => "The member's membership has expired.",
            ErrorCode.OUTSTANDING_FINE => "The member has outstanding fines and cannot borrow books.",
            ErrorCode.LOAN_LIMIT_EXCEEDED => "The member has reached the maximum number of active loans.",
            ErrorCode.BOOK_NOT_AVAILABLE => "The book is currently not available for loan.",
            ErrorCode.ALREADY_RETURNED => "The loan has already been returned.",
            ErrorCode.LOAN_NOT_FOUND => "The requested loan was not found.",
            ErrorCode.CONCURRENCY_CONFLICT => "A concurrency conflict occurred.",
            _ => "An unexpected error occurred."
        };
    }

    public static int GetHttpStatusCode(this ErrorCode errorCode)
    {
        return errorCode switch
        {
            ErrorCode.BOOK_NOT_FOUND => 404,
            ErrorCode.DUPLICATE_ISBN => 409,
            ErrorCode.MEMBER_NOT_FOUND => 404,
            ErrorCode.DUPLICATE_EMAIL => 409,
            ErrorCode.MEMBERSHIP_EXPIRED => 400,
            ErrorCode.OUTSTANDING_FINE => 400,
            ErrorCode.LOAN_LIMIT_EXCEEDED => 400,
            ErrorCode.BOOK_NOT_AVAILABLE => 400,
            ErrorCode.ALREADY_RETURNED => 400,
            ErrorCode.LOAN_NOT_FOUND => 404,
            ErrorCode.CONCURRENCY_CONFLICT => 409,
            _ => 500
        };
    }
}
