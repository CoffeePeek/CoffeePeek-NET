using Microsoft.AspNetCore.Http;

namespace CoffeePeek.Shared.Extensions.Exceptions;

public class NotFoundException : BaseException
{
    public NotFoundException(string message, string? errorCode = null)
        : base(message, errorCode, StatusCodes.Status404NotFound)
    {
    }

    public NotFoundException(string message, Exception innerException, string? errorCode = null)
        : base(message, innerException, errorCode, StatusCodes.Status404NotFound)
    {
    }
}

