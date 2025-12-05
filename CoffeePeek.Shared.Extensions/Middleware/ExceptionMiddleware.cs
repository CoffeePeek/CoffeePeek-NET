using System.Text.Json;
using CoffeePeek.Shared.Extensions.Exceptions;
using CoffeePeek.Shared.Extensions.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace CoffeePeek.Shared.Extensions.Middleware;

public class ExceptionMiddleware(
    RequestDelegate next,
    ILogger<ExceptionMiddleware> logger,
    IWebHostEnvironment environment)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = context.TraceIdentifier;
        var errorResponse = CreateErrorResponse(exception, traceId);

        logger.LogError(
            exception,
            "Exception occurred. TraceId: {TraceId}, Path: {Path}, Method: {Method}",
            traceId,
            context.Request.Path,
            context.Request.Method);

        context.Response.StatusCode = errorResponse.StatusCode;
        context.Response.ContentType = "application/json";

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = environment.IsDevelopment()
        };

        var json = JsonSerializer.Serialize(errorResponse, jsonOptions);
        await context.Response.WriteAsync(json);
    }

    private ErrorResponse CreateErrorResponse(Exception exception, string traceId)
    {
        var errorResponse = new ErrorResponse
        {
            TraceId = traceId,
            Message = GetSafeMessage(exception),
            StatusCode = GetStatusCode(exception),
            ErrorCode = GetErrorCode(exception)
        };

        if (environment.IsDevelopment())
        {
            errorResponse.StackTrace = exception.StackTrace;
            if (exception.InnerException != null)
            {
                errorResponse.InnerException = exception.InnerException.ToString();
            }
        }

        if (exception is ValidationException { Errors: not null } validationException)
        {
            errorResponse.Errors = validationException.Errors;
        }

        return errorResponse;
    }

    private string GetSafeMessage(Exception exception)
    {
        return exception switch
        {
            BaseException baseException => baseException.Message,
            NpgsqlException => "Database connection error. Please try again later.",
            UnauthorizedAccessException => "Unauthorized access.",
            _ => environment.IsDevelopment()
                ? exception.Message
                : "An error occurred while processing your request."
        };
    }

    private int GetStatusCode(Exception exception)
    {
        return exception switch
        {
            BaseException { StatusCode: not null } baseException => baseException.StatusCode.Value,
            ValidationException or BusinessException => StatusCodes.Status400BadRequest,
            NotFoundException => StatusCodes.Status404NotFound,
            UnauthorizedException => StatusCodes.Status401Unauthorized,
            DatabaseException or NpgsqlException => StatusCodes.Status503ServiceUnavailable,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError
        };
    }

    private static string? GetErrorCode(Exception exception)
    {
        return exception is BaseException baseException ? baseException.ErrorCode : null;
    }
}