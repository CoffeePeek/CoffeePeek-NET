using System.Reflection;
using CoffeePeek.Data.Extensions;
using CoffeePeek.Shared.Extensions.Configuration;
using CoffeePeek.Shared.Extensions.Middleware;
using CoffeePeek.Shared.Extensions.Modules;
using CoffeePeek.Shared.Extensions.Swagger;
using CoffeePeek.ShopsService.Configuration;
using CoffeePeek.ShopsService.Consumers;
using CoffeePeek.ShopsService.DB;
using CoffeePeek.ShopsService.Entities;
using CoffeePeek.ShopsService.Extensions;
using CoffeePeek.ShopsService.Services;
using CoffeePeek.ShopsService.Services.Interfaces;
using CoffeePeek.Shared.Extensions.Logging;
using CoffeePeek.Shared.Extensions.Outbox;
using CoffeePeek.Shared.Infrastructure.Options;

var builder = WebApplication.CreateBuilder(args);

builder.AddSerilogLogging();

// Environment configuration
builder.ConfigureEnvironment();

// Controllers and API
builder.Services.AddControllersModule();

// Swagger
builder.Services.AddSwaggerModule("CoffeePeek.ShopsService Service", "v1");

// MediatR
builder.Services.AddMediatRModule(Assembly.GetExecutingAssembly());

// JWT Authentication
builder.Services.AddJwtAuthModule();

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(CoffeePeek.Shared.Infrastructure.Constants.RoleConsts.Admin, policy => policy.RequireRole(CoffeePeek.Shared.Infrastructure.Constants.RoleConsts.Admin));
    options.AddPolicy(CoffeePeek.Shared.Infrastructure.Constants.RoleConsts.User, policy => policy.RequireRole(CoffeePeek.Shared.Infrastructure.Constants.RoleConsts.User));
    options.AddPolicy(CoffeePeek.Shared.Infrastructure.Constants.RoleConsts.Merchant, policy => policy.RequireRole(CoffeePeek.Shared.Infrastructure.Constants.RoleConsts.Merchant));
});

// Cache
builder.Services.AddCacheModule();

// Mapster
builder.Services.AddSingleton(MapsterConfiguration.CreateMapper());

// Validation
builder.Services.AddValidators();

// Cache service
builder.Services.AddScoped<ICacheService, CacheService>();

// Messaging for publishing events
builder.Services.AddMessagingModule(x =>
{
    x.AddConsumer<CoffeeShopApprovedEventConsumer>();
});

// Outbox Event Publisher
builder.Services.AddOutboxEventPublisher<OutboxEvent, ShopsDbContext>();

// Database
var dbOptions = builder.Services.GetDatabaseOptions();
builder.Services.AddEfCoreData<ShopsDbContext>(dbOptions.ConnectionString ?? DatabaseConnectionHelper.GetDatabaseConnectionString());
builder.Services.AddGenericRepository<Shop, ShopsDbContext>();
builder.Services.AddGenericRepository<ShopContact, ShopsDbContext>();
builder.Services.AddGenericRepository<ShopPhoto, ShopsDbContext>();
builder.Services.AddGenericRepository<City, ShopsDbContext>();
builder.Services.AddGenericRepository<Review, ShopsDbContext>();
builder.Services.AddGenericRepository<FavoriteShop, ShopsDbContext>();
builder.Services.AddGenericRepository<CoffeeBean, ShopsDbContext>();
builder.Services.AddGenericRepository<Equipment, ShopsDbContext>();
builder.Services.AddGenericRepository<BrewMethod, ShopsDbContext>();
builder.Services.AddGenericRepository<Roaster, ShopsDbContext>();
builder.Services.AddGenericRepository<Location, ShopsDbContext>();

// Database Seeder
builder.Services.AddScoped<DatabaseSeederService>();

// Health Checks
var dbOptionsForHealth = builder.Services.GetDatabaseOptions();
var rabbitMqOptionsForHealth = builder.Services.AddValidateOptions<RabbitMqOptions>();
var redisOptionsForHealth = builder.Services.AddValidateOptions<RedisOptions>();
builder.Services.AddAllHealthChecks(dbOptionsForHealth, rabbitMqOptionsForHealth, redisOptionsForHealth);

// CORS
builder.Services.AddCorsModule();

var app = builder.Build();

// Middleware pipeline
app.UseExceptionHandling();

if (CorsModule.IsCorsEnabled())
{
    app.UseCors();
}

// Swagger documentation
app.UseSwaggerDocumentation();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Health Checks
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("self")
});

app.MapControllers();

app.Run();
