using CoffeePeek.MediaService.BackgroundJobs;
using CoffeePeek.MediaService.Configuration;
using CoffeePeek.MediaService.Data;
using CoffeePeek.MediaService.Handlers;
using CoffeePeek.MediaService.Services;
using CoffeePeek.Shared.Auth.Constants;
using CoffeePeek.Shared.Auth.Extensions;
using CoffeePeek.Shared.Domain.Interfaces.Persistance;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Extentions;
using CoffeePeek.Shared.Persistence;
using CoffeePeek.Shared.Persistence.Data;
using CoffeePeek.Shared.Persistence.Extensions;
using CoffeePeek.Shared.Web.Handlers;
using Microsoft.EntityFrameworkCore;
using Minio;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add authentication and authorization
builder.Services.AddHeaderUserContext();
builder.Services.AddAuthorizationBuilder()
    .AddPolicy(RoleConsts.Admin, policy => policy.RequireRole(RoleConsts.Admin))
    .AddPolicy(RoleConsts.User, policy => policy.RequireRole(RoleConsts.User));

// Add services
builder.Services.AddScoped<IStorageService, MinIOStorageService>();
builder.Services.AddScoped<IPhotoService, PhotoService>();
builder.Services.AddScoped<PhotoCleanupService>();

// Register CAP handlers
builder.Services.AddScoped<PhotoReplacedEventHandler>();

// Add background job
builder.Services.AddHostedService<PhotoCleanupBackgroundJob>();

// Configure MinIO
var minIoOptions = builder.Services.AddValidateOptions<MinIOOptions>();
builder.Services
    .AddMinio(configureClient =>
        configureClient
            .WithEndpoint(new Uri(minIoOptions.Endpoint))
            .WithCredentials(minIoOptions.AccessKey, minIoOptions.SecretKey)
            .Build()
    );

string connectionString;
if (builder.Configuration["DOTNET_ASPIRE"] == "true")
{
    builder.AddNpgsqlDbContext<MediaDbContext>(
        connectionName: AppResources.MediaDb,
        configureSettings: settings => { settings.DisableRetry = true; }
        );
    connectionString = builder.Configuration.GetConnectionString(AppResources.MediaDb);
}
else
{
    connectionString = builder.Services.AddValidateOptions<PostgresCpOptions>().ConnectionString;
    builder.Services.AddDbContext<MediaDbContext>(opt => opt.UseNpgsql(connectionString));
}

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //await app.ApplyMigrations<MediaDbContext>();
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
