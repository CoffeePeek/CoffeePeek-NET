using CoffeePeek.Gateway;
using CoffeePeek.Gateway.Extensions;
using CoffeePeek.Shared.Auth.Constants;
using CoffeePeek.Shared.Web.Extensions;
using CoffeePeek.Shared.Web.Handlers;
using CoffeePeek.Shared.Web.Logging;
using CoffePeek.ServiceDefaults;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi(options =>
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
builder.AddServiceDefaults();

builder.Services.AddServiceDiscovery();

var proxyBuilder = builder.Services.AddReverseProxy()
    .LoadFromMemory(YarpRouteFactory.CreateRoutes(), YarpClusterFactory.CreateClusters())
    .AddServiceDiscoveryDestinationResolver();

// Добавляем трансформы к существующему билдеру, а не создаем новый
proxyBuilder.AddTransforms<ClaimsToHeadersTransformProvider>();

builder.AddSerilogLogging();

builder.ConfigureEnvironment();

// JWT Authentication for Gateway
builder.Services.AddGatewayJwtAuth();

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(RoleConsts.Admin, policy => policy.RequireRole(RoleConsts.Admin))
    .AddPolicy(RoleConsts.Owner, policy => policy.RequireRole(RoleConsts.Owner))
    .AddPolicy(RoleConsts.User, policy => policy.RequireRole(RoleConsts.User))
    .AddPolicy(RoleConsts.Moderator, policy => policy.RequireRole(RoleConsts.Moderator))
    .AddPolicy(RoleConsts.Employee, policy => policy.RequireRole(RoleConsts.Employee))
    .AddPolicy(RoleConsts.Roaster, policy => policy.RequireRole(RoleConsts.Roaster));

builder.Services.AddResponseCaching();
builder.Services.AddCorsModule();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();
var token =
        "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJhYzAwMzE3Ny04YmYyLTQ3YTItODEyYS05NTk2Zjg0OGIxZmMiLCJuYW1lIjoiWmFseXBhNDQiLCJlbWFpbCI6InN0ZWZpc2VuQHlhbmRleC5ydSIsImp0aSI6IjhhZDhlZmI1LTY4YTgtNDQ3NC04MmFkLTk4ZTM2ZTVkODAxNyIsInByZWZlcnJlZF91c2VybmFtZSI6IlphbHlwYTQ0IiwiZW1haWxfdmVyaWZpZWQiOiJzdGVmaXNlbkB5YW5kZXgucnUiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOlsiTW9kZXJhdG9yIiwiVXNlciIsIkFkbWluIl0sImV4cCI6MTc3MTQ3MTg4OSwiaXNzIjoiQ29mZmVlUGVlay5XRUIiLCJhdWQiOiJDb2ZmZWVQZWVrLkFQSSJ9.weTXcaU6NQhAXQ3vOETHz1lEhUsaQywLM6WhaJaNrUY";
    
app.MapScalarApiReference(o =>
{
    o.Theme = ScalarTheme.Moon;
    o.DefaultHttpClient = new KeyValuePair<ScalarTarget, ScalarClient>(ScalarTarget.CSharp, ScalarClient.HttpClient);
    o.AddPreferredSecuritySchemes("Bearer");
    
    o.AddHttpAuthentication("Bearer", auth =>
    {
        auth.Token = token; 
    }).EnablePersistentAuthentication();
    
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

app.MapDefaultEndpoints();

app.UseExceptionHandler();

app.UseCors();

// Authentication & Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.UseResponseCaching();

// Gateway self health check
app.MapGet("/health/gateway", () => Results.Ok(new { status = "healthy", service = "Gateway", timestamp = DateTime.UtcNow }))
    .WithName("GatewayHealthCheck")
    .WithTags("Health");

app.MapReverseProxy();

app.Run();
