using System.Net;

namespace CoffeePeek.Shared.Kernel.Exceptions;

public class ValidationException : BaseException
{
    public Dictionary<string, string[]>? Errors { get; }

    public ValidationException(string message, string? errorCode = null)
        : base(message, errorCode, (int)HttpStatusCode.BadRequest)
    {
    }

    public ValidationException(string message, Dictionary<string, string[]> errors, string? errorCode = null)
        : base(message, errorCode, (int)HttpStatusCode.BadRequest)
    {
        Errors = errors;
    }

    public ValidationException(string message, Exception innerException, string? errorCode = null)
        : base(message, innerException, errorCode, (int)HttpStatusCode.BadRequest)
    {
    }
}