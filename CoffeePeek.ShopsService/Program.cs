using System.Reflection;
using CoffeePeek.Data.Extensions;
using CoffeePeek.Shared.Extensions.Configuration;
using CoffeePeek.Shared.Extensions.Middleware;
using CoffeePeek.Shared.Extensions.Modules;
using CoffeePeek.Shared.Extensions.Swagger;
using CoffeePeek.ShopsService.Configuration;
using CoffeePeek.ShopsService.DB;
using CoffeePeek.ShopsService.Entities;
using CoffeePeek.ShopsService.Entities.CheckIn;
using CoffeePeek.ShopsService.Extensions;
using CoffeePeek.ShopsService.Services;
using CoffeePeek.ShopsService.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

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

// Cache
builder.Services.AddCacheModule();

// Mapster
builder.Services.AddSingleton(MapsterConfiguration.CreateMapper());

// Validation
builder.Services.AddValidators();

// Cache service
builder.Services.AddScoped<ICacheService, CacheService>();

// Messaging for publishing events
builder.Services.AddMessagingModule();

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

// Database Seeder
builder.Services.AddScoped<DatabaseSeederService>();

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

app.UseAuthorization();

app.MapControllers();

app.Run();
