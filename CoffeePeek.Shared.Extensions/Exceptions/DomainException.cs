namespace CoffeePeek.Shared.Extensions.Exceptions;

public class DomainException : BaseException
{
    public DomainException(string message, string? errorCode = null, int? statusCode = null) : base(message, errorCode,
        statusCode)
    {
    }

    public DomainException(string message, Exception innerException, string? errorCode = null, int? statusCode = null) :
        base(message, innerException, errorCode, statusCode)
    {
    }
}