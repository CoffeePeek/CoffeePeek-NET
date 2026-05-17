using CoffeePeek.Shared.Kernel.Exceptions;
using CoffeePeek.Shared.Kernel.Response;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CoffeePeek.Shared.Web.Handlers;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IWebHostEnvironment environment) 
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var traceId = httpContext.TraceIdentifier;

        logger.LogError(
            exception,
            "Exception occurred: {Message}. TraceId: {TraceId}",
            exception.Message, traceId);

        var statusCode = GetStatusCode(exception);

        ErrorResponse errorResponse;
        if (environment.IsDevelopment())
        {
            errorResponse = new ErrorResponse(GetSafeMessage(exception))
            {
                StackTrace = exception.StackTrace,
                InnerException = exception.InnerException?.Message
            };
        }
        else
        {
            errorResponse = new ErrorResponse(GetSafeMessage(exception));
        }

        httpContext.Response.StatusCode = statusCode;
        
        await httpContext.Response.WriteAsJsonAsync(errorResponse, cancellationToken);

        return true;
    }

    private static int GetStatusCode(Exception exception) => exception switch
    {
        BaseException { StatusCode: not null } be => be.StatusCode.Value,
        NotFoundException => StatusCodes.Status404NotFound,
        UnauthorizedException or UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
        ValidationException or DomainException => StatusCodes.Status400BadRequest,
        ConflictException => StatusCodes.Status409Conflict,
        DatabaseException => StatusCodes.Status503ServiceUnavailable,
        _ => StatusCodes.Status500InternalServerError
    };

    private string GetSafeMessage(Exception exception) => exception switch
    {
        BaseException be => be.Message,
        _ when environment.IsDevelopment() => exception.Message,
        _ => "An unexpected error occurred."
    };
}
