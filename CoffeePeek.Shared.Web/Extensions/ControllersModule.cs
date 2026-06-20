using CoffeePeek.Shared.Kernel.ErrorCodes;
using CoffeePeek.Shared.Kernel.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeePeek.Shared.Web.Extensions;

public static class ControllersModule
{
    public static IServiceCollection AddControllersModule(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();

        // Map ASP.NET model binding / data annotation errors to our ErrorResponse format
        // so the frontend always gets a consistent JSON shape regardless of the error source.
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var fieldErrors = context.ModelState
                    .Where(entry => entry.Value?.Errors.Count > 0)
                    .ToDictionary(
                        entry => entry.Key,
                        entry => entry.Value!.Errors
                            .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage)
                                ? "Invalid value."
                                : e.ErrorMessage)
                            .ToArray()
                    );

                var errorResponse = new ErrorResponse(
                    "One or more validation errors occurred.",
                    CommonErrorCodes.ValidationFailed,
                    fieldErrors);

                return new BadRequestObjectResult(errorResponse);
            };
        });

        return services;
    }
}

