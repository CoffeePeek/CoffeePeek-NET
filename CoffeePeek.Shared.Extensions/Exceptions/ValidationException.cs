using Microsoft.AspNetCore.Http;

namespace CoffeePeek.Shared.Extensions.Exceptions;

public class ValidationException : BaseException
{
    public Dictionary<string, string[]>? Errors { get; }

    public ValidationException(string message, string? errorCode = null)
        : base(message, errorCode, StatusCodes.Status400BadRequest)
    {
    }

    public ValidationException(string message, Dictionary<string, string[]> errors, string? errorCode = null)
        : base(message, errorCode, StatusCodes.Status400BadRequest)
    {
        Errors = errors;
    }

    public ValidationException(string message, Exception innerException, string? errorCode = null)
        : base(message, innerException, errorCode, StatusCodes.Status400BadRequest)
    {
    }
}