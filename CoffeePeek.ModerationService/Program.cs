using Coffeepeek.Moderation.Application.Mapper;
using Coffeepeek.Moderation.Application.Repositories;
using CoffeePeek.Moderation.Domain;
using CoffeePeek.Moderation.Domain.Entities;
using CoffeePeek.Moderation.Domain.Repositories;
using CoffeePeek.Moderation.Infrastructure;
using CoffeePeek.Moderation.Infrastructure.Services;
using CoffeePeek.ModerationService.Services;
using CoffeePeek.ModerationService.Services.Interfaces;
using CoffeePeek.Shared.Extensions.Configuration;
using CoffeePeek.Shared.Extensions.Middleware;
using CoffeePeek.Shared.Extensions.Modules;
using CoffeePeek.Shared.Extensions.Resilience;
using CoffeePeek.Shared.Extensions.Swagger;
using CoffeePeek.Shared.Infrastructure.Constants;
using CoffeePeek.Shared.Extensions.Logging;
using CoffeePeek.Shared.Extensions.Outbox;
using CoffePeek.ServiceDefaults;
using OutboxEvent = CoffeePeek.Moderation.Domain.Entities.OutboxEvent;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddSerilogLogging();

// Environment configuration (PORT, AllowedHosts, etc.)
builder.ConfigureEnvironment();

// Controllers and Swagger
builder.Services.AddControllersModule();
builder.Services.AddSwaggerModule("Coffee Peek ModerationService", "v1");

// Database
var dbOptions = builder.Services.GetDatabaseOptions(builder.Configuration, databaseName: AppResources.ModerationDb);
builder.Services.AddEfCoreData<ModerationDbContext>(dbOptions);

builder.Services.AddGenericRepository<ModerationShop, ModerationDbContext>();
builder.Services.AddGenericRepository<ShopContacts, ModerationDbContext>();
builder.Services.AddGenericRepository<Location, ModerationDbContext>();
builder.Services.AddGenericRepository<ModerationShopSchedule, ModerationDbContext>();
builder.Services.AddGenericRepository<ModerationShopEquipment, ModerationDbContext>();
builder.Services.AddGenericRepository<ModerationCoffeeBeanShop, ModerationDbContext>();
builder.Services.AddGenericRepository<ModerationRoasterShop, ModerationDbContext>();
builder.Services.AddGenericRepository<ModerationShopBrewMethod, ModerationDbContext>();

builder.Services.AddScoped<IModerationShopRepository, ModerationShopRepository>();
builder.Services.AddScoped<IModerationShopCreationService, ModerationShopCreationService>();
builder.Services.AddScoped<IModerationScheduleService, ModerationScheduleService>();
builder.Services.AddScoped<IModerationRelationsService, ModerationRelationsService>();

// Mapster
builder.Services.AddSingleton(MapsterConfiguration.CreateMapper());

// Yandex Geocoding Service
var yandexOptions = builder.Services.AddValidateOptions<YandexApiOptions>();
builder.Services.AddHttpClient<IYandexGeocodingService, YandexGeocodingService>(client =>
{
    client.BaseAddress = new Uri(yandexOptions.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(yandexOptions.TimeoutSeconds);
}).AddResiliencePolicies(nameof(YandexGeocodingService));

// MediatR
builder.Services.AddMediatRModule(typeof(Program));

// JWT Authentication
builder.Services.AddJwtAuthModule();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(RoleConsts.Admin, policy => policy.RequireRole(RoleConsts.Admin));
    options.AddPolicy(RoleConsts.Merchant, policy => policy.RequireRole(RoleConsts.Merchant));
    options.AddPolicy(RoleConsts.User, policy => policy.RequireRole(RoleConsts.User));
});

// RabbitMQ для публикации событий
builder.Services.AddMessagingModule();

// Outbox Event Publisher
builder.Services.AddOutboxEventPublisher<OutboxEvent, ModerationDbContext>();

// CORS
builder.Services.AddCorsModule();

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseExceptionHandling();

// Use CORS if configured
if (CorsModule.IsCorsEnabled())
{
    app.UseCors();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseSwaggerDocumentation();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
