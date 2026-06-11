namespace LibrarySystem.Services.Exceptions;

public class BusinessRuleViolationException : LibraryException
{
    public BusinessRuleViolationException(ErrorCode errorCode) : base(errorCode) { }
    
    public BusinessRuleViolationException(ErrorCode errorCode, string message) : base(errorCode, message) { }
}
