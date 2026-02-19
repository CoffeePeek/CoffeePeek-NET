using CoffeePeek.MediaService.Configuration;
using CoffeePeek.MediaService.Data;
using CoffeePeek.MediaService.Handlers;
using CoffeePeek.MediaService.Services;
using CoffeePeek.Shared.Auth.Constants;
using CoffeePeek.Shared.Auth.Extensions;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Extentions;
using CoffeePeek.Shared.Persistence;
using CoffeePeek.Shared.Persistence.Data;
using CoffeePeek.Shared.Persistence.Extensions;
using CoffeePeek.Shared.Web;
using CoffeePeek.Shared.Web.Handlers;
using Minio;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecurityTransformer>();
});

builder.Services.AddHeaderUserContext();
builder.Services.AddAuthorizationBuilder()
    .AddPolicy(RoleConsts.Admin, policy => policy.RequireRole(RoleConsts.Admin))
    .AddPolicy(RoleConsts.User, policy => policy.RequireRole(RoleConsts.User));

builder.Services.AddScoped<IStorageService, MinIOStorageService>();

var minIoOptions = builder.Services.AddValidateOptions<MinIOOptions>();
builder.Services
    .AddMinio(configureClient =>
        configureClient
            .WithEndpoint(new Uri(minIoOptions.Endpoint))
            .WithCredentials(minIoOptions.AccessKey, minIoOptions.SecretKey)
            .Build()
    );

#if DEBUG
builder.AddNpgsqlDbContext<MediaDbContext>(
    connectionName: AppResources.MediaDb,
    configureDbContextOptions: opt => opt.AddInterceptors(new AuditInterceptor()),
    configureSettings: settings => { settings.DisableRetry = true; }
);
    
builder.Services.AddScoped<IUnitOfWork, UnitOfWork<MediaDbContext>>();
#else
        var connectionString = GetConnectionString(services);

        services.AddDatabase<ModerationDbContext>(
            connectionString,
            opt => opt.AddInterceptors(new AuditInterceptor())
        );
        
        static string GetConnectionString(IServiceCollection services)
        {
            return services.AddValidateOptions<PostgresCpOptions>().ConnectionString;
        }
#endif

var handlersAssembly = typeof(ConfirmPhotoHandler).Assembly;
builder.Host.AddWolverine(handlersAssembly);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();

app.Run();