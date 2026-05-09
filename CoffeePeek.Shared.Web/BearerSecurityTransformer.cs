using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace CoffeePeek.Shared.Web;

public class BearerSecurityTransformer(IApiDescriptionGroupCollectionProvider apiDescriptions)
    : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken ct)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Enter token"
        };

        document.Servers ??= [];
        document.Servers.Clear();
        document.Servers.Add(new OpenApiServer { Url = "/", Description = "Gateway" });

        var schemeReference = new OpenApiSecuritySchemeReference("Bearer", document);

        var authorizedOperations = apiDescriptions.ApiDescriptionGroups.Items
            .SelectMany(g => g.Items)
            .Where(api =>
                api.ActionDescriptor.EndpointMetadata.OfType<IAuthorizeData>().Any() &&
                !api.ActionDescriptor.EndpointMetadata.OfType<IAllowAnonymous>().Any())
            .Select(api => $"{api.HttpMethod?.ToUpper()}:/{api.RelativePath?.TrimEnd('/')}")
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var (path, pathItem) in document.Paths)
        {
            foreach (var (operationType, operation) in pathItem.Operations)
            {
                var key = $"{operationType.ToString().ToUpper()}:{path}";
            
                if (!authorizedOperations.Contains(key))
                    continue;

                operation.Security = new List<OpenApiSecurityRequirement>
                {
                    new() { [schemeReference] = [] }
                };
            }
        }

        return Task.CompletedTask;
    }
}
