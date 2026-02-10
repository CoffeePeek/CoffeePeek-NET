using Microsoft.AspNetCore.Http;

namespace CoffeePeek.Shared.Kernel.Exceptions;

public class DatabaseException : BaseException
{
    public DatabaseException(string message, string? errorCode = null)
        : base(message, errorCode, StatusCodes.Status503ServiceUnavailable)
    {
    }

    public DatabaseException(string message, Exception innerException, string? errorCode = null)
        : base(message, innerException, errorCode, StatusCodes.Status503ServiceUnavailable)
    {
    }
}