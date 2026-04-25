using CoffeePeek.MediaService.Configuration;
using CoffeePeek.MediaService.Consumers;
using CoffeePeek.MediaService.Data;
using CoffeePeek.MediaService.Handlers;
using CoffeePeek.MediaService.Repositories;
using CoffeePeek.MediaService.Services;
using CoffeePeek.Shared.Auth.Constants;
using CoffeePeek.Shared.Auth.Extensions;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Extentions;
using CoffeePeek.Shared.Persistence;
using CoffeePeek.Shared.Persistence.Data;
using CoffeePeek.Shared.Persistence.Extensions;
using CoffeePeek.Shared.Web;
using CoffeePeek.Shared.Web.Extensions;
using CoffeePeek.Shared.Web.Handlers;
using CoffeePeek.Shared.Web.Logging;
using CoffePeek.ServiceDefaults;
using Minio;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

builder.AddSerilogLogging();
builder.AddServiceDefaults();
builder.WebHost.ConfigureEnvironment();
builder.WebHost.UseSentry();

services.AddControllers();
services.AddProblemDetails();
services.AddExceptionHandler<GlobalExceptionHandler>();
services.AddHeaderUserContext();

services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecurityTransformer>();
});

services.AddAuthorizationBuilder()
    .AddPolicy(RoleConsts.Admin, policy => policy.RequireRole(RoleConsts.Admin))
    .AddPolicy(RoleConsts.User, policy => policy.RequireRole(RoleConsts.User));

#if DEBUG
builder.AddNpgsqlDbContext<MediaDbContext>(
    connectionName: AppResources.MediaDb,
    configureDbContextOptions: opt => opt.AddInterceptors(new AuditInterceptor()),
    configureSettings: settings => { settings.DisableRetry = true; }
);
#else
var connectionString = services.AddValidateOptions<PostgresCpOptions>().ConnectionString;

services.AddDatabase<MediaDbContext>(
    connectionString,
    opt => opt.AddInterceptors(new AuditInterceptor())
);
#endif

services.AddScoped<IUnitOfWork, UnitOfWork<MediaDbContext>>();
services.AddScoped<IPhotoRepository, PhotoRepository>();

var minIoOptions = services.AddValidateOptions<MinIOOptions>();

services.AddScoped<IStorageService, MinIOStorageService>();
services.AddMinio(client => client
    .WithEndpoint(new Uri(minIoOptions.Endpoint))
    .WithCredentials(minIoOptions.AccessKey, minIoOptions.SecretKey)
    .Build()
);


var handlersAssembly = typeof(ConfirmPhotoHandler).Assembly;
builder.AddWolverine([handlersAssembly]);

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapDefaultEndpoints();
app.MapControllers();

app.Run();
