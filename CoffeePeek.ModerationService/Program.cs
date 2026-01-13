using Coffeepeek.Moderation.Application.Abstractions;
using CoffeePeek.Moderation.Application.Common;
using Coffeepeek.Moderation.Application.Features.CreateShop;
using Coffeepeek.Moderation.Application.Features.Review.SendReviewToModeration;
using CoffeePeek.Moderation.Application.Features.Review.SendReviewToModeration;
using Coffeepeek.Moderation.Application.Features.Shop.CreateShop;
using Coffeepeek.Moderation.Application.Features.Shop.GetAllModerationShops;
using CoffeePeek.Moderation.Domain.Entities;
using CoffeePeek.Moderation.Domain.Entities.ModerationReviewAggregate;
using CoffeePeek.Moderation.Domain.Repositories;
using CoffeePeek.Moderation.Infrastructure;
using CoffeePeek.Moderation.Infrastructure.Mapper;
using CoffeePeek.Moderation.Infrastructure.Services;
using CoffeePeek.Shared.Extensions.Configuration;
using CoffeePeek.Shared.Extensions.Handlers;
using CoffeePeek.Shared.Extensions.Modules;
using CoffeePeek.Shared.Extensions.Resilience;
using CoffeePeek.Shared.Extensions.Swagger;
using CoffeePeek.Shared.Infrastructure.Constants;
using CoffeePeek.Shared.Extensions.Logging;
using CoffeePeek.Shared.Extensions.Outbox;
using CoffeePeek.Shared.Infrastructure.Abstract.S3;
using CoffeePeek.Shared.Validation;
using CoffePeek.ServiceDefaults;
using Minio;
using OutboxEvent = CoffeePeek.Moderation.Domain.Entities.OutboxEvent;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseSentry();

builder.AddServiceDefaults();

builder.AddSerilogLogging();

// Environment configuration (PORT, AllowedHosts, etc.)
builder.ConfigureEnvironment();

// Controllers and Swagger
builder.Services.AddControllersModule();
builder.Services.AddSwaggerModule("Coffee Peek ModerationService");

// Database
var dbOptions = builder.Services.GetDatabaseOptions(builder.Configuration, databaseName: AppResources.ModerationDb);
builder.Services.AddEfCoreData<ModerationDbContext, OutboxEvent>(dbOptions);

builder.Services.AddGenericRepository<ModerationShop, ModerationDbContext>();
builder.Services.AddGenericRepository<ModerationShopContact, ModerationDbContext>();
builder.Services.AddGenericRepository<ModerationLocation, ModerationDbContext>();
builder.Services.AddGenericRepository<ModerationShopSchedule, ModerationDbContext>();
builder.Services.AddGenericRepository<ModerationShopEquipment, ModerationDbContext>();
builder.Services.AddGenericRepository<ModerationCoffeeBeanShop, ModerationDbContext>();
builder.Services.AddGenericRepository<ModerationShopRoaster, ModerationDbContext>();
builder.Services.AddGenericRepository<ModerationShopBrewMethod, ModerationDbContext>();
builder.Services.AddGenericRepository<PhotoMetadata, ModerationDbContext>();
builder.Services.AddGenericRepository<ModerationReview, ModerationDbContext>();

builder.Services.AddScoped<IModerationShopRepository, ModerationShopRepository>();
builder.Services.AddScoped<IModerationShopCreationService, ModerationShopCreationService>();
builder.Services.AddScoped<IModerationReviewRepository, ModerationReviewRepository>();

builder.Services.AddScoped<IStorageService, MinIOStorageService>();

builder.Services.AddTransient<IValidationStrategy<SendReviewToModerationCommand>, SendReviewToModerationValidationStrategy>();

// Mapster
builder.Services.AddSingleton(MapsterConfiguration.CreateMapper());

// Yandex Geocoding Service
var yandexOptions = builder.Services.AddValidateOptions<YandexApiOptions>();
builder.Services.AddHttpClient<IYandexGeocodingService, YandexGeocodingService>(client =>
{
    client.BaseAddress = new Uri(yandexOptions.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(yandexOptions.TimeoutSeconds);
}).AddResiliencePolicies(nameof(YandexGeocodingService));

var minIoOptions = builder.Services.AddValidateOptions<MinIOOptions>();
builder.Services
    .AddMinio(configureClient => 
        configureClient
            .WithEndpoint(minIoOptions.Endpoint)
            .WithCredentials(minIoOptions.AccessKey, minIoOptions.SecretKey)
            .Build()
        );

// MediatR
builder.Services.AddMediatRModule(typeof(GetAllModerationShopsHandler));

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

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();

app.MapDefaultEndpoints();

app.UseAuthentication();
app.UseAuthorization();

app.UseSwaggerDocumentation();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
