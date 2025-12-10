using CoffeePeek.Data.Extensions;
using CoffeePeek.Shared.Extensions.Middleware;
using CoffeePeek.Shared.Extensions.Modules;
using CoffeePeek.Shared.Extensions.Swagger;
using CoffeePeek.UserService.Configuration;
using CoffeePeek.UserService.EventConsumer;
using CoffeePeek.UserService.Models;
using CoffeePeek.UserService.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Environment configuration
builder.ConfigureEnvironment();

// Controllers and API
builder.Services.AddControllersModule();

// Swagger
builder.Services.AddSwaggerModule("Coffee Peek UserService", "v1");

// Messaging
builder.Services.AddMessagingModule(x =>
{
    x.AddConsumer<UserRegisteredEventConsumer>();
    x.AddConsumer<CheckinCreatedEventConsumer>();
    x.AddConsumer<ReviewAddedEventConsumer>();
    x.AddConsumer<CoffeeShopApprovedEventConsumer>();
});

// Database
var dbOptions = builder.Services.GetDatabaseOptions();
builder.Services.AddEfCoreData<UserDbContext>(dbOptions);
builder.Services.AddGenericRepository<User, UserDbContext>();

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Cache
builder.Services.AddCacheModule();

// Mapster
builder.Services.AddSingleton(MapsterConfiguration.CreateMapper());

// MediatR
builder.Services.AddMediatRModule(typeof(Program));

// JWT Authentication
builder.Services.AddJwtAuthModule();

// CORS
builder.Services.AddCorsModule();

var app = builder.Build();

// Middleware pipeline
app.UseExceptionHandling();

if (CorsModule.IsCorsEnabled())
{
    app.UseCors();
}

app.UseAuthentication();
app.UseAuthorization();

// Swagger documentation
app.UseSwaggerDocumentation();

app.MapControllers();

app.Run();