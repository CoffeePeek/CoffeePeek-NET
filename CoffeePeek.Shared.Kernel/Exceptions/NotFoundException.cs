using System.Net;

namespace CoffeePeek.Shared.Kernel.Exceptions;

public class NotFoundException : BaseException
{
    public NotFoundException(string message, string? errorCode = null)
        : base(message, errorCode, (int)HttpStatusCode.NotFound)
    {
    }

    public NotFoundException(string message, Exception innerException, string? errorCode = null)
        : base(message, innerException, errorCode, (int)HttpStatusCode.NotFound)
    {
    }
}