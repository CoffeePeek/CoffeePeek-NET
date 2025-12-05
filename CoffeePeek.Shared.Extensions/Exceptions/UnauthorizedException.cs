using Microsoft.AspNetCore.Http;

namespace CoffeePeek.Shared.Extensions.Exceptions;

public class UnauthorizedException : BaseException
{
    public UnauthorizedException(string message = "Unauthorized access", string? errorCode = null)
        : base(message, errorCode, StatusCodes.Status401Unauthorized)
    {
    }

    public UnauthorizedException(string message, Exception innerException, string? errorCode = null)
        : base(message, innerException, errorCode, StatusCodes.Status401Unauthorized)
    {
    }
}