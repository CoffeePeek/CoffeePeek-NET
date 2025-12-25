using CoffeePeek.Account.Application.Commands;
using CoffeePeek.Account.Application.Common.Interfaces;
using CoffeePeek.Account.Application.Features.Login;
using CoffeePeek.Account.Application.Features.OAuthLogin;
using CoffeePeek.Account.Application.Mapper;
using CoffeePeek.Account.Domain.Aggregates.UserAggregate;
using CoffeePeek.Account.Domain.Entities;
using CoffeePeek.Account.Domain.Repositories;
using CoffeePeek.Account.Domain.Services;
using CoffeePeek.Auth.Infrastructure;
using CoffeePeek.Auth.Infrastructure.EventConsumer;
using CoffeePeek.Auth.Infrastructure.ExternalServices;
using CoffeePeek.Auth.Infrastructure.Identity;
using CoffeePeek.Auth.Infrastructure.Persistent;
using CoffeePeek.Auth.Infrastructure.Persistent.Repositories;
using CoffeePeek.Shared.Extensions.Configuration;
using CoffeePeek.Shared.Extensions.Middleware;
using CoffeePeek.Shared.Extensions.Modules;
using CoffeePeek.Shared.Extensions.Swagger;
using CoffeePeek.Shared.Infrastructure.Constants;
using CoffeePeek.Shared.Extensions.Logging;
using CoffeePeek.Shared.Extensions.Outbox;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.UserService.Models;
using CoffePeek.ServiceDefaults;
using JWTOptions = CoffeePeek.Shared.Infrastructure.Options.JWTOptions;
using JWTTokenService = CoffeePeek.Auth.Infrastructure.Identity.JWTTokenService;
using OutboxEvent = CoffeePeek.Account.Domain.Entities.OutboxEvent;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddSerilogLogging();

// Environment configuration
builder.ConfigureEnvironment();

// Controllers and API
builder.Services.AddControllersModule();

// Swagger
builder.Services.AddSwaggerModule("CoffeePeek.AccountService", "v1");

// Authentication & Authorization
builder.Services.AddCookieAuthModule();
builder.Services.AddValidateOptions<JWTOptions>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(RoleConsts.Admin, policy => policy.RequireRole(RoleConsts.Admin));
    options.AddPolicy(RoleConsts.User, policy => policy.RequireRole(RoleConsts.User));
});

// Database
var dbOptions = builder.Services.GetDatabaseOptions(builder.Configuration, databaseName: AppResources.AccountDb);
builder.Services.AddEfCoreData<AccountDbContext>(dbOptions);
builder.Services.AddGenericRepository<UserCredential, AccountDbContext>();
builder.Services.AddGenericRepository<OutboxEvent, AccountDbContext>();
builder.Services.AddGenericRepository<Role, AccountDbContext>();
builder.Services.AddGenericRepository<UserStatistics, AccountDbContext>();
builder.Services.AddGenericRepository<User, AccountDbContext>();

// Messaging - only consumers for external service events (Shops)
builder.Services.AddMessagingModule(x =>
{
    x.AddConsumer<CheckinCreatedEventConsumer>();
    x.AddConsumer<ReviewAddedEventConsumer>();
    x.AddConsumer<CoffeeShopApprovedAccountConsumer>();
});

// Mapster
builder.Services.AddSingleton(MapsterConfiguration.CreateMapper());

// Application Services
builder.Services.AddScoped<IJWTTokenService, JWTTokenService>();
builder.Services.AddScoped<IPasswordHasherService, PasswordHasherService>();
builder.Services.AddScoped<IUserCredentialsRepository, UserCredentialsRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IExternalAuthService, ExternalAuthService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<UserQueries>();
builder.Services.AddScoped<IUserQueries>(provider =>
{
    var baseService = provider.GetRequiredService<UserQueries>();
    var redis = provider.GetRequiredService<IRedisService>();
    return new CachedUserQueries(baseService, redis);
});

builder.Services.AddScoped<UserRepository>(); 

builder.Services.AddScoped<IUserRepository>(provider => 
{
    var baseRepo = provider.GetRequiredService<UserRepository>();
    var cache = provider.GetRequiredService<IRedisService>(); 
    
    return new CachedUserRepository(baseRepo, cache);
});
// OAuth
builder.Services.AddValidateOptions<OAuthGoogleOptions>();
builder.Services.AddScoped<IGoogleAuthService, GoogleAuthService>();

// HTTP Context
builder.Services.AddHttpContextAccessor();

// Cache
builder.Services.AddCacheModule();

// MediatR
builder.Services.AddMediatRModule(
    typeof(LoginUserHandler)
);

// Outbox Event Publisher
builder.Services.AddOutboxEventPublisher<OutboxEvent, AccountDbContext>();

// CORS
builder.Services.AddCorsModule();

var app = builder.Build();

app.MapDefaultEndpoints();

// Middleware pipeline
app.UseExceptionHandling();

if (CorsModule.IsCorsEnabled())
{
    app.UseCors();
}

// Swagger documentation
app.UseSwaggerDocumentation();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
