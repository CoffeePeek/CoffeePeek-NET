using System.Reflection;
using CoffeePeek.AuthService.Commands;
using CoffeePeek.AuthService.Configuration;
using CoffeePeek.AuthService.Entities;
using CoffeePeek.AuthService.Repositories;
using CoffeePeek.AuthService.Services;
using CoffeePeek.AuthService.Services.Validation;
using CoffeePeek.Data.Extensions;
using CoffeePeek.Shared.Extensions.Configuration;
using CoffeePeek.Shared.Extensions.Middleware;
using CoffeePeek.Shared.Extensions.Options;
using CoffeePeek.Shared.Extensions.Swagger;
using CoffeePeek.Shared.Infrastructure.Interfaces.Redis;
using CoffeePeek.Shared.Infrastructure.Services;
using CoffeePeek.BuildingBlocks.AuthOptions;
using CoffeePeek.Shared.Infrastructure;
using CoffeePeek.Shared.Infrastructure.Options;
using MassTransit;
using Microsoft.AspNetCore.Authentication.Cookies;
using IJWTTokenService = CoffeePeek.AuthService.Services.IJWTTokenService;
using JWTOptions = CoffeePeek.Shared.Infrastructure.Options.JWTOptions;
using JWTTokenService = CoffeePeek.AuthService.Services.JWTTokenService;

var builder = WebApplication.CreateBuilder(args);

#region PORT

// Configure PORT from environment variable
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port) && int.TryParse(port, out var portNumber))
{
    builder.WebHost.UseUrls($"http://*:{portNumber}");
}

#endregion

#region ALLOWED_HOSTS

// Configure AllowedHosts from environment variable
var allowedHosts = Environment.GetEnvironmentVariable("ALLOWED_HOSTS");
if (!string.IsNullOrEmpty(allowedHosts))
{
    builder.Configuration["AllowedHosts"] = allowedHosts;
}

#endregion

#region Endpoints

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();

#endregion

//Swagger
builder.Services.AddSwagger("CoffeePeek.AuthService", "v1");

builder.Services.AddValidateOptions<JWTOptions>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(RoleConsts.Admin, policy => policy.RequireRole(RoleConsts.Admin));
    options.AddPolicy(RoleConsts.User, policy => policy.RequireRole(RoleConsts.User));
});

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultSignOutScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
    });

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var connectionString = DatabaseConnectionHelper.GetDatabaseConnectionString();

var dbOptions = builder.Services.AddValidateOptions<PostgresCpOptions>();
if (!string.IsNullOrEmpty(connectionString))
{
    dbOptions.ConnectionString = connectionString;
}

builder.Services.AddEfCoreData<AuthDbContext>(dbOptions);
builder.Services.AddGenericRepository<UserCredentials, AuthDbContext>();
builder.Services.AddGenericRepository<OutboxEvent, AuthDbContext>();
builder.Services.AddGenericRepository<Role, AuthDbContext>();

builder.Services.AddScoped<IJWTTokenService, JWTTokenService>();
builder.Services.AddScoped<IPasswordHasherService, PasswordHasherService>();
builder.Services.AddScoped<IRoleManager, RoleManager>();
builder.Services.AddScoped<IUserManager, UserManager>();
builder.Services.AddScoped<ISessionManager, SessionManager>();
builder.Services.AddScoped<ISignInManager, SignInManager>();
builder.Services.AddScoped<IUserCredentialsRepository, UserCredentialsRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();

builder.Services.AddValidateOptions<OAuthGoogleOptions>();
builder.Services.AddScoped<IGoogleAuthService, GoogleAuthService>();

builder.Services.AddHttpContextAccessor();

builder.Services.RedisConfigurationOptions();
builder.Services.AddTransient<IRedisService, RedisService>();

builder.Services.AddTransient<IValidationStrategy<RegisterUserCommand>, UserCreateValidationStrategy>();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
});

var rabbitMqOptions = builder.Services.AddValidateOptions<RabbitMqOptions>();
var railwayRabbitMqOptions = RabbitMqConnectionHelper.GetRabbitMqOptions();
if (railwayRabbitMqOptions != null)
{
    rabbitMqOptions.HostName = railwayRabbitMqOptions.HostName;
    rabbitMqOptions.Port = railwayRabbitMqOptions.Port;
    rabbitMqOptions.Username = railwayRabbitMqOptions.Username;
    rabbitMqOptions.Password = railwayRabbitMqOptions.Password;
}

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbitMqOptions.HostName, rabbitMqOptions.Port, "/", h =>
        {
            h.Username(rabbitMqOptions.Username);
            h.Password(rabbitMqOptions.Password);
        });

        cfg.ConfigureEndpoints(context);
    });
});

// Configure CORS from environment variable
var allowedOrigins = Environment.GetEnvironmentVariable("ALLOWED_ORIGINS");
if (!string.IsNullOrEmpty(allowedOrigins))
{
    var origins = allowedOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.WithOrigins(origins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
    });
}

var app = builder.Build();


// Configure the HTTP request pipeline.
app.UseExceptionHandling();

if (!string.IsNullOrEmpty(allowedOrigins))
{
    app.UseCors();
}

// Check IS_DEBUG environment variable
var isDebug = Environment.GetEnvironmentVariable("IS_DEBUG");
var showSwagger = app.Environment.IsDevelopment() || 
                  (!string.IsNullOrEmpty(isDebug) && isDebug.Equals("true", StringComparison.OrdinalIgnoreCase));

if (showSwagger)
{
    app.MapOpenApi();
    app.UseSwagger();
    
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();