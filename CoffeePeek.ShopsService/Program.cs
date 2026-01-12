using System.Reflection;
using CoffeePeek.Shared.Extensions.Configuration;
using CoffeePeek.Shared.Extensions.Handlers;
using CoffeePeek.Shared.Extensions.Modules;
using CoffeePeek.Shared.Extensions.Swagger;
using CoffeePeek.Shared.Extensions.Logging;
using CoffeePeek.Shared.Extensions.Outbox;
using CoffeePeek.Shops.Application.Common;
using CoffeePeek.Shops.Application.Features.CoffeeShop.GetCoffeeShop;
using CoffeePeek.Shops.Application.Mapper;
using CoffeePeek.Shops.Application.Services;
using CoffeePeek.Shops.Domain.Entities;
using CoffeePeek.Shops.Infrastructure.Configuration;
using CoffeePeek.Shops.Infrastructure.Consumers;
using CoffeePeek.Shops.Infrastructure.Extensions;
using CoffeePeek.Shops.Infrastructure.Services;
using CoffePeek.ServiceDefaults;
using Mapster;
using OutboxEvent = CoffeePeek.Shops.Domain.Entities.OutboxEvent;
using Review = CoffeePeek.Shops.Domain.Entities.ReviewAggregate.Review;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseSentry(options =>
{
    options.Dsn = builder.Configuration["Sentry:Dsn"];
    options.SendDefaultPii = true;
    options.SetBeforeSend((@event, _) =>
    {
        @event.ServerName = null;
        return @event;
    });
});

builder.AddServiceDefaults();

builder.AddSerilogLogging();

// Environment configuration
builder.ConfigureEnvironment();

// Controllers and API
builder.Services.AddControllersModule();

// Swagger
builder.Services.AddSwaggerModule("CoffeePeek.ShopsService Service");

// MediatR
builder.Services.AddMediatRModule(Assembly.GetExecutingAssembly());

// MediatR
builder.Services.AddMediatRModule(typeof(GetCoffeeShopHandler));

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
builder.Services.AddMapster();
var config = TypeAdapterConfig.GlobalSettings;
config.Scan(typeof(MapsterConfiguration).Assembly);

// Validation
builder.Services.AddValidators();

// Cache service
builder.Services.AddScoped<ICacheService, CacheService>();
builder.Services.AddScoped<ICreateShopFromModerationService, CreateShopFromModerationService>();
builder.Services.AddScoped<IShopCacheService, ShopCacheService>();

// Messaging for publishing events
builder.Services.AddMessagingModule(x =>
{
    x.AddConsumer<CoffeeShopApprovedShopsConsumer>();
});

// Outbox Event Publisher
builder.Services.AddOutboxEventPublisher<OutboxEvent, ShopsDbContext>();

// Database
var dbOptions = builder.Services.GetDatabaseOptions(builder.Configuration, databaseName: AppResources.ShopsDb);
builder.Services.AddEfCoreData<ShopsDbContext, OutboxEvent>(dbOptions.ConnectionString);
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
builder.Services.AddGenericRepository<CheckIn, ShopsDbContext>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();


var app = builder.Build();

app.UseExceptionHandler();

app.MapDefaultEndpoints();

// Swagger documentation
app.UseSwaggerDocumentation();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
