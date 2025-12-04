using System.Reflection;
using CoffeePeek.Data.Extensions;
using CoffeePeek.Shared.Extensions.Configuration;
using CoffeePeek.Shared.Extensions.Middleware;
using CoffeePeek.Shared.Extensions.Swagger;
using CoffeePeek.Shared.Infrastructure.Interfaces.Redis;
using CoffeePeek.Shared.Infrastructure.Options;
using CoffeePeek.Shared.Infrastructure.Services;
using CoffeePeek.ShopsService.Configuration;
using CoffeePeek.ShopsService.DB;
using CoffeePeek.ShopsService.Entities;
using CoffeePeek.ShopsService.Extensions;
using CoffeePeek.ShopsService.Services;
using CoffeePeek.ShopsService.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Swagger
builder.Services.AddSwagger("CoffeePeek.ShopsService Service", "v1");

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
});

// Redis configuration
builder.Services.RedisConfigurationOptions();
builder.Services.AddScoped<IRedisService, RedisService>();

// Mapster
builder.Services.AddSingleton(MapsterConfiguration.CreateMapper());

// Validation
builder.Services.AddValidators();

// Cache service
builder.Services.AddScoped<ICacheService, CacheService>();

#region Database

var connectionString = DatabaseConnectionHelper.GetDatabaseConnectionString();
var dbOptions = builder.Services.AddValidateOptions<PostgresCpOptions>();

if (!string.IsNullOrEmpty(connectionString))
{
    dbOptions.ConnectionString = connectionString;
}

builder.Services.AddEfCoreData<ShopsDbContext>(dbOptions.ConnectionString ?? connectionString);
builder.Services.AddGenericRepository<Shop, ShopsDbContext>();
builder.Services.AddGenericRepository<ShopContact, ShopsDbContext>();
builder.Services.AddGenericRepository<ShopPhoto, ShopsDbContext>();
builder.Services.AddGenericRepository<City, ShopsDbContext>();
builder.Services.AddGenericRepository<Review, ShopsDbContext>();
builder.Services.AddGenericRepository<FavoriteShop, ShopsDbContext>();

// Database Seeder
builder.Services.AddScoped<DatabaseSeederService>();

#endregion

var app = builder.Build();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeederService>();
    await seeder.SeedAsync();
}

// Configure the HTTP request pipeline.
app.UseExceptionHandling();

app.UseSwaggerDocumentation();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();