using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using CoffeePeek.Account.Application;
using CoffeePeek.Account.Application.Common;
using CoffeePeek.Account.Application.Common.Interfaces;
using CoffeePeek.Account.Application.Features.Auth.Login;
using CoffeePeek.Account.Application.Features.Auth.OAuthLogin;
using CoffeePeek.Account.Application.Mapper;
using CoffeePeek.Account.Domain.Entities;
using CoffeePeek.Account.Domain.Entities.RoleAggregate;
using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Account.Domain.Services;
using CoffeePeek.Auth.Infrastructure;
using CoffeePeek.Auth.Infrastructure.Configuration;
using CoffeePeek.Auth.Infrastructure.EventConsumer;
using CoffeePeek.Auth.Infrastructure.Identity;
using CoffeePeek.Auth.Infrastructure.Repositories;
using CoffeePeek.Moderation.Infrastructure;
using CoffeePeek.Shared.Extensions.Configuration;
using CoffeePeek.Shared.Extensions.Handlers;
using CoffeePeek.Shared.Extensions.Modules;
using CoffeePeek.Shared.Extensions.Swagger;
using CoffeePeek.Shared.Infrastructure.Constants;
using CoffeePeek.Shared.Extensions.Logging;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Abstract.S3;
using CoffePeek.ServiceDefaults;
using Microsoft.OpenApi;
using Minio;
using Resend;
using JWTOptions = CoffeePeek.Shared.Infrastructure.Options.JWTOptions;
using JWTTokenService = CoffeePeek.Auth.Infrastructure.Identity.JWTTokenService;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseSentry();

builder.AddServiceDefaults();
builder.AddSerilogLogging();

// Environment configuration
builder.ConfigureEnvironment();

// Controllers and API
builder.Services.AddControllersModule();

// Swagger
builder.Services.AddJwtAuthModule();
builder.Services.AddValidateOptions<JWTOptions>();
builder.Services.AddAuthorizationBuilder()
    .AddPolicy(RoleConsts.Admin, policy => policy.RequireRole(RoleConsts.Admin))
    .AddPolicy(RoleConsts.User, policy => policy.RequireRole(RoleConsts.User));

builder.Services.AddSwaggerModule("CoffeePeek Account Service");

// Database
var dbOptions = builder.Services.GetDatabaseOptions(builder.Configuration, databaseName: AppResources.AccountDb);
builder.Services.AddEfCoreData<AccountDbContext>(dbOptions);
builder.Services.AddGenericRepository<UserCredential, AccountDbContext>();
builder.Services.AddGenericRepository<Role, AccountDbContext>();
builder.Services.AddGenericRepository<UserStatistics, AccountDbContext>();
builder.Services.AddGenericRepository<User, AccountDbContext>();
builder.Services.AddGenericRepository<PhotoMetadata, AccountDbContext>();

// Resend
builder.Services.AddHttpClient<ResendClient>();
builder.Services.Configure<ResendClientOptions>( o =>
{
    o.ApiToken = builder.Configuration["ResendApi"]!;
} );
builder.Services.AddTransient<IResend, ResendClient>();

// CAP for event publishing and consuming
builder.Services.AddCapModule<AccountDbContext>(dbOptions, "account-service");

// Register CAP handlers
builder.Services.AddScoped<CheckinCreatedHandler>();
builder.Services.AddScoped<ReviewAddedHandler>();
builder.Services.AddScoped<ModerationShopApprovedAccountHandler>();

// Mapster
builder.Services.AddSingleton(MapsterConfiguration.CreateMapper());

// Application Services
builder.Services.AddScoped<IJWTTokenService, JWTTokenService>();
builder.Services.AddScoped<IPasswordHasherService, PasswordHasherService>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IQueryUserRepository, QueryUserRepository>();
builder.Services.AddScoped<IExternalAuthService, ExternalAuthService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddSingleton<EmailExistenceFilter>(_ => 
{
    const int expectedCount = 1000000; 
    const double errorRate = 0.01;
    
    return new EmailExistenceFilter(expectedCount, errorRate);
});

builder.Services.AddSingleton<IEmailTemplateService, EmailTemplateService>();

builder.Services.AddScoped<UserRepository>(); 

builder.Services.AddScoped<IUserRepository>(provider => 
{
    var baseRepo = provider.GetRequiredService<UserRepository>();
    var redisService = provider.GetRequiredService<IRedisService>(); 
    
    return new CachedUserRepository(baseRepo, redisService);
});

builder.Services.AddScoped<IStorageService, MinIOStorageService>();
var minIoOptions = builder.Services.AddValidateOptions<MinIOOptions>();
builder.Services
    .AddMinio(configureClient => 
        configureClient
            .WithEndpoint(minIoOptions.Endpoint)
            .WithCredentials(minIoOptions.AccessKey, minIoOptions.SecretKey)
            .Build()
    );


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


builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// Middleware pipeline
app.UseExceptionHandler();

app.MapDefaultEndpoints();

app.UseAuthentication();
app.UseAuthorization();

// Swagger documentation
app.UseSwaggerDocumentation();

app.MapControllers();

app.Run();