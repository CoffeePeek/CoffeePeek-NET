using CoffeePeek.Shared.Extensions.Exceptions;
using CoffeePeek.Shared.Extensions.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace CoffeePeek.Shared.Extensions.Handlers;

public class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IWebHostEnvironment environment) : IExceptionHandler
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
        
        var errorResponse = new ErrorResponse
        {
            TraceId = traceId,
            StatusCode = statusCode,
            Message = GetSafeMessage(exception),
            ErrorCode = (exception as BaseException)?.ErrorCode ?? "INTERNAL_SERVER_ERROR"
        };

        if (exception is ValidationException { Errors: not null } valEx)
        {
            errorResponse.Errors = valEx.Errors;
        }

        if (environment.IsDevelopment())
        {
            errorResponse.StackTrace = exception.StackTrace;
            errorResponse.InnerException = exception.InnerException?.Message;
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
        DatabaseException or NpgsqlException => StatusCodes.Status503ServiceUnavailable,
        _ => StatusCodes.Status500InternalServerError
    };

    private string GetSafeMessage(Exception exception) => exception switch
    {
        BaseException be => be.Message,
        NpgsqlException => "Database error. Please try again later.",
        _ when environment.IsDevelopment() => exception.Message,
        _ => "An unexpected error occurred."
    };
}