using Microsoft.AspNetCore.Http;

namespace CoffeePeek.Shared.Extensions.Exceptions;

public class BusinessException : BaseException
{
    public BusinessException(string message, string? errorCode = null)
        : base(message, errorCode, StatusCodes.Status400BadRequest)
    {
    }

    public BusinessException(string message, Exception innerException, string? errorCode = null)
        : base(message, innerException, errorCode, StatusCodes.Status400BadRequest)
    {
    }
}

