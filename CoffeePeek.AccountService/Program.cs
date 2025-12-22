using CoffeePeek.Account.Application.Commands;
using CoffeePeek.Account.Application.Mapper;
using CoffeePeek.Account.Application.Repositories;
using CoffeePeek.Account.Application.Services;
using CoffeePeek.Account.Application.Services.Interfaces;
using CoffeePeek.Account.Application.Services.Validation;
using CoffeePeek.Account.Domain.Entities;
using CoffeePeek.Account.Domain.Repositories;
using CoffeePeek.Auth.Infrastructure;
using CoffeePeek.Auth.Infrastructure.Configuration;
using CoffeePeek.Auth.Infrastructure.EventConsumer;
using CoffeePeek.Auth.Infrastructure.Repositories;
using CoffeePeek.Auth.Infrastructure.Services;
using CoffeePeek.Auth.Infrastructure.Services.Validation;
using CoffeePeek.Shared.Extensions.Configuration;
using CoffeePeek.Shared.Extensions.Middleware;
using CoffeePeek.Shared.Extensions.Modules;
using CoffeePeek.Shared.Extensions.Swagger;
using CoffeePeek.Shared.Infrastructure.Constants;
using CoffeePeek.Shared.Extensions.Logging;
using CoffeePeek.Shared.Extensions.Outbox;
using CoffeePeek.User.Domain.Repositories;
using CoffeePeek.UserService.Models;
using CoffePeek.ServiceDefaults;
using JWTOptions = CoffeePeek.Shared.Infrastructure.Options.JWTOptions;
using JWTTokenService = CoffeePeek.Auth.Infrastructure.Services.JWTTokenService;
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
    x.AddConsumer<CoffeeShopApprovedEventConsumer>();
});

// Mapster
builder.Services.AddSingleton(MapsterConfiguration.CreateMapper());

// Application Services
builder.Services.AddScoped<IJWTTokenService, JWTTokenService>();
builder.Services.AddScoped<IPasswordHasherService, PasswordHasherService>();
builder.Services.AddScoped<IRoleManager, RoleManager>();
builder.Services.AddScoped<IUserManager, UserManager>();
builder.Services.AddScoped<ISessionManager, SessionManager>();
builder.Services.AddScoped<ISignInManager, SignInManager>();
builder.Services.AddScoped<IUserCredentialsRepository, UserCredentialsRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// OAuth
builder.Services.AddValidateOptions<OAuthGoogleOptions>();
builder.Services.AddScoped<IGoogleAuthService, GoogleAuthService>();

// HTTP Context
builder.Services.AddHttpContextAccessor();

// Cache
builder.Services.AddCacheModule();

// Validation
builder.Services.AddTransient<IValidationStrategy<RegisterUserCommand>, UserCreateValidationStrategy>();

// MediatR
builder.Services.AddMediatRModule(
    typeof(CoffeePeek.Account.Application.Handlers.LoginUserHandler)
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
