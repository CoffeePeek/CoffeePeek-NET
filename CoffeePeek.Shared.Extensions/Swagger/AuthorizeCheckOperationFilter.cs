using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CoffeePeek.Shared.Extensions.Swagger;

public class AuthorizeCheckOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasAuthAttributes = context.MethodInfo.DeclaringType?.GetCustomAttributes(true)
            .Union(context.MethodInfo.GetCustomAttributes(true))
            .OfType<AuthorizeAttribute>()
            .Any() ?? false;

        if (!hasAuthAttributes)
        {
            return;
        }

        operation.Responses ??= new OpenApiResponses();
        operation.Responses.Add("401", new OpenApiResponse { Description = "Unauthorized" });

        var schemeReference = new OpenApiSecuritySchemeReference("Bearer", context.Document);

        operation.Security = new List<OpenApiSecurityRequirement>
        {
            new()
            {
                [schemeReference] = []
            }
        };
    }
}