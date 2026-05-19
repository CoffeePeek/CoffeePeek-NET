using System.Net;

namespace CoffeePeek.Shared.Kernel.Exceptions;

public class ForbiddenException : BaseException
{
    public ForbiddenException(string message = "Access forbidden", string? errorCode = null)
        : base(message, errorCode, (int)HttpStatusCode.Forbidden)
    {
    }

    public ForbiddenException(string message, Exception innerException, string? errorCode = null)
        : base(message, innerException, errorCode, (int)HttpStatusCode.Forbidden)
    {
    }
}
