using System.Net;

namespace CoffeePeek.Shared.Kernel.Exceptions;

public class UnauthorizedException : BaseException
{
    public UnauthorizedException(string message = "Unauthorized access", string? errorCode = null)
        : base(message, errorCode, (int)HttpStatusCode.Unauthorized)
    {
    }

    public UnauthorizedException(string message, Exception innerException, string? errorCode = null)
        : base(message, innerException, errorCode, (int)HttpStatusCode.Unauthorized)
    {
    }
}