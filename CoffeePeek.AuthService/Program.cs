using System.Reflection;
using CoffeePeek.AuthService.Commands;
using CoffeePeek.AuthService.Configuration;
using CoffeePeek.AuthService.Entities;
using CoffeePeek.AuthService.Repositories;
using CoffeePeek.AuthService.Services;
using CoffeePeek.AuthService.Services.Validation;
using CoffeePeek.AuthService.Utils;
using CoffeePeek.Data.Extensions;
using CoffeePeek.Shared.Extensions.Configuration;
using CoffeePeek.Shared.Extensions.Middleware;
using CoffeePeek.Shared.Extensions.Modules;
using CoffeePeek.Shared.Extensions.Swagger;
using CoffeePeek.Shared.Infrastructure.Constants;
using CoffeePeek.Shared.Extensions.Logging;
using CoffeePeek.Shared.Extensions.Outbox;
using CoffeePeek.Shared.Infrastructure.Options;
using IJWTTokenService = CoffeePeek.AuthService.Services.IJWTTokenService;
using JWTOptions = CoffeePeek.Shared.Infrastructure.Options.JWTOptions;
using JWTTokenService = CoffeePeek.AuthService.Services.JWTTokenService;

var builder = WebApplication.CreateBuilder(args);
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
var dbOptions = builder.Services.GetDatabaseOptions();
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

// Health Checks
var dbOptionsForHealth = builder.Services.GetDatabaseOptions();
var rabbitMqOptionsForHealth = builder.Services.AddValidateOptions<RabbitMqOptions>();
var redisOptionsForHealth = builder.Services.AddValidateOptions<RedisOptions>();
builder.Services.AddAllHealthChecks(dbOptionsForHealth, rabbitMqOptionsForHealth, redisOptionsForHealth);

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

app.UseAuthentication();
app.UseAuthorization();

// Health Checks
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("self")
});

app.MapControllers();

app.Run();
