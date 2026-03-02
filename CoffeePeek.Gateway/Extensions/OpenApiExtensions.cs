using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

namespace CoffeePeek.Gateway.Extensions;

/// <summary>
/// Extension methods for configuring OpenAPI documentation and the Scalar UI
/// aggregated across all downstream microservices.
/// </summary>
public static class OpenApiExtensions
{
    /// <summary>
    /// Registers the OpenAPI document generator with Bearer JWT security scheme support.
    /// </summary>
    public static IServiceCollection AddGatewayOpenApi(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, _, _) =>
            {
                document.Components ??= new OpenApiComponents();

                document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Description = "Enter JWT token"
                };

                return Task.CompletedTask;
            });

            options.AddOperationTransformer((operation, context, _) =>
            {
                var hasAuthorize =
                    context.Description.ActionDescriptor.EndpointMetadata
                        .OfType<AuthorizeAttribute>()
                        .Any();

                if (!hasAuthorize)
                    return Task.CompletedTask;

                operation.Security ??= new List<OpenApiSecurityRequirement>();

                var schemeReference = new OpenApiSecuritySchemeReference("Bearer");

                operation.Security.Add(new OpenApiSecurityRequirement
                {
                    [schemeReference] = []
                });

                return Task.CompletedTask;
            });
        });

        return services;
    }

    /// <summary>
    /// Maps the Scalar API reference UI with documents for all four downstream services.
    /// </summary>
    public static IApplicationBuilder UseGatewayScalarUi(this WebApplication app)
    {
        app.MapScalarApiReference(o =>
        {
            o.Theme = ScalarTheme.Moon;
            o.DefaultHttpClient = new KeyValuePair<ScalarTarget, ScalarClient>(ScalarTarget.CSharp, ScalarClient.HttpClient);
            o.AddPreferredSecuritySchemes("Bearer");

            o.AddHttpAuthentication("Bearer", _ => { }).EnablePersistentAuthentication();

            o.AddDocument(
                documentName: "account",
                title: "Account API",
                routePattern: "/account/openapi/v1.json",
                isDefault: true);

            o.AddDocument(
                documentName: "shops",
                title: "Shops API",
                routePattern: "/shops/openapi/v1.json");

            o.AddDocument(
                documentName: "moderation",
                title: "Moderation API",
                routePattern: "/moderation/openapi/v1.json");

            o.AddDocument(
                documentName: "media",
                title: "Media API",
                routePattern: "/media/openapi/v1.json");
        });

        return app;
    }
}
