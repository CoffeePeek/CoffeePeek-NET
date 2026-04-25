using System.Net;

namespace CoffeePeek.Shared.Kernel.Exceptions;

public class DatabaseException : BaseException
{
    public DatabaseException(string message, string? errorCode = null)
        : base(message, errorCode, (int)HttpStatusCode.ServiceUnavailable)
    {
    }

    public DatabaseException(string message, Exception innerException, string? errorCode = null)
        : base(message, innerException, errorCode, (int)HttpStatusCode.ServiceUnavailable)
    {
    }
}