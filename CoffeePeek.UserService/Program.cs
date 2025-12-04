using System.Text;
using CoffeePeek.Data.Extensions;
using CoffeePeek.Shared.Extensions.Configuration;
using CoffeePeek.Shared.Extensions.Middleware;
using CoffeePeek.Shared.Extensions.Options;
using CoffeePeek.Shared.Extensions.Swagger;
using CoffeePeek.Shared.Infrastructure.Interfaces.Redis;
using CoffeePeek.Shared.Infrastructure.Options;
using CoffeePeek.Shared.Infrastructure.Services;
using CoffeePeek.UserService.Configuration;
using CoffeePeek.UserService.EventConsumer;
using CoffeePeek.UserService.Models;
using CoffeePeek.UserService.Repositories;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Swagger
builder.Services.AddSwagger("Coffee Peek UserService", "v1");

var rabbitMqOptions = builder.Services.AddValidateOptions<RabbitMqOptions>();
// Переопределяем опции если они получены из Railway переменных окружения
var railwayRabbitMqOptions = RabbitMqConnectionHelper.GetRabbitMqOptions();
if (railwayRabbitMqOptions != null)
{
    rabbitMqOptions.HostName = railwayRabbitMqOptions.HostName;
    rabbitMqOptions.Port = railwayRabbitMqOptions.Port;
    rabbitMqOptions.Username = railwayRabbitMqOptions.Username;
    rabbitMqOptions.Password = railwayRabbitMqOptions.Password;
}

// Настройка строки подключения к БД с поддержкой Railway переменных окружения
var connectionString = DatabaseConnectionHelper.GetDatabaseConnectionString();

var dbOptions = builder.Services.AddValidateOptions<PostgresCpOptions>();
// Переопределяем ConnectionString если она получена из Railway переменных
if (!string.IsNullOrEmpty(connectionString))
{
    dbOptions.ConnectionString = connectionString;
}

builder.Services.AddEfCoreData<UserDbContext>(dbOptions);
builder.Services.AddGenericRepository<User, UserDbContext>();

builder.Services.AddScoped<IUserRepository, UserRepository>();

// Redis
builder.Services.RedisConfigurationOptions();
builder.Services.AddScoped<IRedisService, RedisService>();

// MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// JWT Authentication
var authOptions = builder.Services.AddValidateOptions<JWTOptions>();
builder.Services.AddAuthorization();

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

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<UserRegisteredEventConsumer>();
    
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

// Configure the HTTP request pipeline.
app.UseExceptionHandling();

app.UseAuthentication();
app.UseAuthorization();

app.UseSwaggerDocumentation();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();