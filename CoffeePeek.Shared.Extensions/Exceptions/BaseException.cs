namespace CoffeePeek.Shared.Extensions.Exceptions;

public abstract class BaseException : Exception
{
    public string? ErrorCode { get; }
    public int? StatusCode { get; }

    protected BaseException(string message, string? errorCode = null, int? statusCode = null)
        : base(message)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
    }

    protected BaseException(string message, Exception innerException, string? errorCode = null, int? statusCode = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
    }
}

