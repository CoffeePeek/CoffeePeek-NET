using CoffeePeek.Contract.Response;
using CoffeePeek.Shops.Exceptions;
using NpgsqlException = Npgsql.NpgsqlException;

namespace CoffeePeek.Shops.Middleware;

public class ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (NpgsqlException ex)
        {
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            await context.Response.WriteAsJsonAsync(new Response<object> { Success = false, Message = ex.Message });
        }
        catch (NotFoundException ex)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await context.Response.WriteAsJsonAsync(new Response<object> { Success = false, Message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new Response<object> { Success = false, Message = "Unauthorized access." });
        }
            
        catch (Exception ex)
        {
            logger.LogError($"Unhandled Exception: {ex}");

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new Response<object> { Success = false, Message = "An error occurred while processing your request." });
        }
    }
}