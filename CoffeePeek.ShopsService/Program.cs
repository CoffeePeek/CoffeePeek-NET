using System.Reflection;
using CoffeePeek.Contract.Abstract;
using CoffeePeek.Shared.Extensions.Configuration;
using CoffeePeek.Shared.Extensions.Handlers;
using CoffeePeek.Shared.Extensions.Logging;
using CoffeePeek.Shared.Extensions.Modules;
using CoffeePeek.Shared.Extensions.Swagger;
using CoffeePeek.Shared.Infrastructure.Constants;
using CoffeePeek.Shops.Application.Common;
using CoffeePeek.Shops.Application.Features.CoffeeShop.GetCoffeeShop;
using CoffeePeek.Shops.Application.Mapper;
using CoffeePeek.Shops.Application.Services;
using CoffeePeek.Shops.Domain.Entities;
using CoffeePeek.Shops.Domain.Entities.CheckInAggregate;
using CoffeePeek.Shops.Domain.Entities.CoffeeShopAggregate;
using CoffeePeek.Shops.Domain.Entities.ReviewAggregate;
using CoffeePeek.Shops.Domain.Entities.UserFavoriteAggregate;
using CoffeePeek.Shops.Infrastructure.Configuration;
using CoffeePeek.Shops.Infrastructure.Consumers;
using CoffeePeek.Shops.Infrastructure.Extensions;
using CoffeePeek.Shops.Infrastructure.Services;
using CoffePeek.ServiceDefaults;
using Mapster;
using Review = CoffeePeek.Shops.Domain.Entities.ReviewAggregate.Review;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseSentry();

builder.AddServiceDefaults();

builder.AddSerilogLogging();

// Environment configuration
builder.ConfigureEnvironment();

// Controllers and API
builder.Services.AddControllersModule();

// Swagger
builder.Services.AddSwaggerModule("Shops Service");

// MediatR
builder.Services.AddMediatRModule(Assembly.GetExecutingAssembly());

// MediatR
builder.Services.AddMediatRModule(typeof(GetCoffeeShopHandler));

// Authorization policies (JWT validation happens in Gateway)
builder.Services.AddHeaderUserContext();
builder.Services.AddAuthorizationBuilder()
    .AddPolicy(RoleConsts.Admin, policy => policy.RequireRole(RoleConsts.Admin))
    .AddPolicy(RoleConsts.Owner, policy => policy.RequireRole(RoleConsts.Owner))
    .AddPolicy(RoleConsts.User, policy => policy.RequireRole(RoleConsts.User))
    .AddPolicy(RoleConsts.Moderator, policy => policy.RequireRole(RoleConsts.Moderator))
    .AddPolicy(RoleConsts.Employee, policy => policy.RequireRole(RoleConsts.Employee))
    .AddPolicy(RoleConsts.Roaster, policy => policy.RequireRole(RoleConsts.Roaster));

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
builder.Services.AddScoped<ICoffeeShopCacheService, CoffeeShopCacheService>();

// Database
var dbOptions = builder.Services.GetDatabaseOptions(builder.Configuration, databaseName: AppResources.ShopsDb);
builder.Services.AddEfCoreData<ShopsDbContext>(dbOptions.ConnectionString);

// CAP for event publishing and consuming
builder.Services.AddCapModule<ShopsDbContext>(dbOptions, "shops-service");

// Register CAP handlers
builder.Services.AddScoped<ModerationShopApprovedHandler>();
builder.Services.AddScoped<ModerationReviewApprovedHandler>();
builder.Services.AddScoped<UserNameChangedHandler>();

builder.Services.AddGenericRepository<CoffeeShop, ShopsDbContext>();
builder.Services.AddGenericRepository<ShopPhoto, ShopsDbContext>();
builder.Services.AddGenericRepository<City, ShopsDbContext>();
builder.Services.AddGenericRepository<Review, ShopsDbContext>();
builder.Services.AddGenericRepository<UserFavorite, ShopsDbContext>();
builder.Services.AddGenericRepository<CoffeeBean, ShopsDbContext>();
builder.Services.AddGenericRepository<Equipment, ShopsDbContext>();
builder.Services.AddGenericRepository<BrewMethod, ShopsDbContext>();
builder.Services.AddGenericRepository<Roaster, ShopsDbContext>();
builder.Services.AddGenericRepository<CheckIn, ShopsDbContext>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddScoped<IUserFavoriteRepository, UserFavoriteRepository>();
builder.Services.AddScoped<ICoffeeShopRepository, CoffeeShopRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();

// Domain Services
builder.Services.AddScoped<IUserFavoriteService, UserFavoriteService>();
builder.Services.AddScoped<IUserCheckInRepository, UserCheckInRepository>();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    await CoffeePeek.Shops.Infrastructure.ShopsDbInitializer.SeedAsync(app.Services);
}

app.MapDefaultEndpoints();

// Swagger documentation
app.UseSwaggerDocumentation();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();