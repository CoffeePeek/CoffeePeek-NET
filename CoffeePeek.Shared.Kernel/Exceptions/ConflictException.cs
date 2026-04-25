namespace CoffeePeek.Shared.Kernel.Exceptions;

public class ConflictException : BaseException
{
    public ConflictException(string message, string? errorCode = null, int? statusCode = null) : base(message,
        errorCode, statusCode)
    {
    }

    public ConflictException(string message, Exception innerException, string? errorCode = null,
        int? statusCode = null) : base(message, innerException, errorCode, statusCode)
    {
    }
}