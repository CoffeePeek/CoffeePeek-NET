using System.Text;
using CoffeePeek.Data.Extensions;
using CoffeePeek.Shared.Extensions.Configuration;
using CoffeePeek.Shared.Extensions.Middleware;
using CoffeePeek.Shared.Extensions.Options;
using CoffeePeek.Shared.Extensions.Swagger;
using CoffeePeek.ModerationService.Configuration;
using CoffeePeek.ModerationService.Models;
using CoffeePeek.ModerationService.Repositories;
using CoffeePeek.ModerationService.Repositories.Interfaces;
using CoffeePeek.Shared.Infrastructure;
using CoffeePeek.Shared.Infrastructure.Options;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Configure PORT from environment variable
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port) && int.TryParse(port, out var portNumber))
{
    builder.WebHost.UseUrls($"http://*:{portNumber}");
}

// Configure AllowedHosts from environment variable
var allowedHosts = Environment.GetEnvironmentVariable("ALLOWED_HOSTS");
if (!string.IsNullOrEmpty(allowedHosts))
{
    builder.Configuration["AllowedHosts"] = allowedHosts;
}

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

builder.Services.AddControllers();

// Swagger
builder.Services.AddSwagger("Coffee Peek ModerationService", "v1");

// Настройка строки подключения к БД с поддержкой Railway переменных окружения
var connectionString = DatabaseConnectionHelper.GetDatabaseConnectionString();

var dbOptions = builder.Services.AddValidateOptions<PostgresCpOptions>();
if (!string.IsNullOrEmpty(connectionString))
{
    dbOptions.ConnectionString = connectionString;
}

builder.Services.AddEfCoreData<ModerationDbContext>(dbOptions);
builder.Services.AddGenericRepository<ModerationShop, ModerationDbContext>();

builder.Services.AddScoped<IModerationShopRepository, ModerationShopRepository>();

// MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// JWT Authentication
var authOptions = builder.Services.AddValidateOptions<JWTOptions>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(RoleConsts.Admin, policy => policy.RequireRole(RoleConsts.Admin));
    options.AddPolicy(RoleConsts.Merchant, policy => policy.RequireRole(RoleConsts.Merchant));
    options.AddPolicy(RoleConsts.User, policy => policy.RequireRole(RoleConsts.User));
});

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(x =>
    {
        x.RequireHttpsMetadata = false;
        x.SaveToken = true;
        x.TokenValidationParameters = new TokenValidationParameters
        {
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authOptions.SecretKey)),
            ValidIssuer = authOptions.Issuer,
            ValidAudience = authOptions.Audience,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// RabbitMQ для публикации событий
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

var app = builder.Build();

app.UseExceptionHandling();

// Use CORS if configured
if (!string.IsNullOrEmpty(allowedOrigins))
{
    app.UseCors();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseSwaggerDocumentation();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();


