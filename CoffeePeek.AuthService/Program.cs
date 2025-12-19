using System.Reflection;
using CoffeePeek.Auth.Application.Services;
using CoffeePeek.Auth.Application.Services.Interfaces;
using CoffeePeek.Auth.Application.Services.Validation;
using CoffeePeek.Auth.Domain.Entities;
using CoffeePeek.Auth.Domain.Repositories;
using CoffeePeek.Auth.Infrastructure;
using CoffeePeek.Auth.Infrastructure.Configuration;
using CoffeePeek.Auth.Infrastructure.Repositories;
using CoffeePeek.Auth.Infrastructure.Services;
using CoffeePeek.AuthService.Commands;
using CoffeePeek.AuthService.Services;
using CoffeePeek.AuthService.Services.Validation;
using CoffeePeek.Shared.Extensions.Configuration;
using CoffeePeek.Shared.Extensions.Middleware;
using CoffeePeek.Shared.Extensions.Modules;
using CoffeePeek.Shared.Extensions.Swagger;
using CoffeePeek.Shared.Infrastructure.Constants;
using CoffeePeek.Shared.Extensions.Logging;
using CoffeePeek.Shared.Extensions.Outbox;
using CoffePeek.ServiceDefaults;
using IJWTTokenService = CoffeePeek.Auth.Application.Services.IJWTTokenService;
using JWTOptions = CoffeePeek.Shared.Infrastructure.Options.JWTOptions;
using JWTTokenService = CoffeePeek.Auth.Infrastructure.Services.JWTTokenService;
using OutboxEvent = CoffeePeek.AuthService.Entities.OutboxEvent;
using Role = CoffeePeek.Auth.Domain.Entities.Role;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddSerilogLogging();

// Environment configuration
builder.ConfigureEnvironment();

// Controllers and API
builder.Services.AddControllersModule();

// Swagger
builder.Services.AddSwaggerModule("CoffeePeek.AuthService", "v1");

// Authentication & Authorization
builder.Services.AddCookieAuthModule();
builder.Services.AddValidateOptions<JWTOptions>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(RoleConsts.Admin, policy => policy.RequireRole(RoleConsts.Admin));
    options.AddPolicy(RoleConsts.User, policy => policy.RequireRole(RoleConsts.User));
});

// Database
var dbOptions = builder.Services.GetDatabaseOptions(builder.Configuration, databaseName: AppResources.AuthDb);
builder.Services.AddEfCoreData<AuthDbContext>(dbOptions);
builder.Services.AddGenericRepository<UserCredentials, AuthDbContext>();
builder.Services.AddGenericRepository<OutboxEvent, AuthDbContext>();
builder.Services.AddGenericRepository<Role, AuthDbContext>();

// Application Services
builder.Services.AddScoped<IJWTTokenService, JWTTokenService>();
builder.Services.AddScoped<IPasswordHasherService, PasswordHasherService>();
builder.Services.AddScoped<IRoleManager, RoleManager>();
builder.Services.AddScoped<IUserManager, UserManager>();
builder.Services.AddScoped<ISessionManager, SessionManager>();
builder.Services.AddScoped<ISignInManager, SignInManager>();
builder.Services.AddScoped<IUserCredentialsRepository, UserCredentialsRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();

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
builder.Services.AddMediatRModule(Assembly.GetExecutingAssembly());

// Messaging
builder.Services.AddMessagingModule();

// Outbox Event Publisher
builder.Services.AddOutboxEventPublisher<OutboxEvent, AuthDbContext>();

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
