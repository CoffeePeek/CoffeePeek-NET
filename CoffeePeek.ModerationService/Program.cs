using CoffeePeek.Moderation.Application.Abstractions;
using CoffeePeek.Moderation.Application.Common;
using CoffeePeek.Moderation.Application.Features.Review.SendReviewToModeration;
using CoffeePeek.Moderation.Application.Features.Review.UpdateCoffeeShopReview;
using CoffeePeek.Moderation.Application.Features.Shop.CreateShop;
using CoffeePeek.Moderation.Application.Features.Shop.GetAllModerationShops;
using CoffeePeek.Moderation.Domain.Entities;
using CoffeePeek.Moderation.Domain.Entities.ModerationReviewAggregate;
using CoffeePeek.Moderation.Infrastructure;
using CoffeePeek.Moderation.Infrastructure.Configuration;
using CoffeePeek.Moderation.Infrastructure.Consumers;
using CoffeePeek.Moderation.Infrastructure.Mapper;
using CoffeePeek.Moderation.Infrastructure.Services;
using CoffeePeek.Shared.Extensions.Configuration;
using CoffeePeek.Shared.Extensions.Handlers;
using CoffeePeek.Shared.Extensions.Logging;
using CoffeePeek.Shared.Extensions.Modules;
using CoffeePeek.Shared.Extensions.Resilience;
using CoffeePeek.Shared.Extensions.Swagger;
using CoffeePeek.Shared.Infrastructure.Constants;
using CoffeePeek.Shared.Infrastructure.Options;
using CoffeePeek.Shared.Validation;
using CoffePeek.ServiceDefaults;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseSentry();

builder.AddServiceDefaults();

builder.AddSerilogLogging();

// Environment configuration (PORT, AllowedHosts, etc.)
builder.ConfigureEnvironment();

// Controllers and Swagger
builder.Services.AddControllersModule();
builder.Services.AddSwaggerModule("CoffeePeek Moderation");

// Database
string connectionString;
if (builder.Configuration["DOTNET_ASPIRE"] == "true")
{
    connectionString = builder.Configuration.GetConnectionString(AppResources.ModerationDb) ?? throw new InvalidOperationException("Connection string not found");
    builder.AddNpgsqlDbContext<ModerationDbContext>(AppResources.ModerationDb, configureDbContextOptions: opt => 
        opt.AddInterceptors(new AuditInterceptor()));
}
else
{
    connectionString = builder.Services.AddValidateOptions<PostgresCpOptions>().ConnectionString;
    builder.Services.AddDbContext<ModerationDbContext>(opt => opt.UseNpgsql(connectionString).AddInterceptors(new AuditInterceptor()));
}

builder.Services.AddScoped<IUnitOfWork, UnitOfWork<ModerationDbContext>>();
builder.Services.AddCapModule(connectionString, AppResources.MediaService);

builder.Services.AddGenericRepository<ModerationShop, ModerationDbContext>();
builder.Services.AddGenericRepository<ModerationShopContact, ModerationDbContext>();
builder.Services.AddGenericRepository<ModerationLocation, ModerationDbContext>();
builder.Services.AddGenericRepository<ModerationShopSchedule, ModerationDbContext>();
builder.Services.AddGenericRepository<ModerationShopEquipment, ModerationDbContext>();
builder.Services.AddGenericRepository<ModerationCoffeeBeanShop, ModerationDbContext>();
builder.Services.AddGenericRepository<ModerationShopRoaster, ModerationDbContext>();
builder.Services.AddGenericRepository<ModerationShopBrewMethod, ModerationDbContext>();
builder.Services.AddGenericRepository<PhotoMetadata, ModerationDbContext>();
builder.Services.AddGenericRepository<ModerationReview, ModerationDbContext>();

builder.Services.AddScoped<IModerationShopRepository, ModerationShopRepository>();
builder.Services.AddScoped<IModerationShopCreationService, ModerationShopCreationService>();
builder.Services.AddScoped<IModerationReviewRepository, ModerationReviewRepository>();

builder.Services.AddTransient<IAsyncValidationStrategy<SendReviewToModerationCommand>, SendReviewToModerationValidationStrategy>();

// Validation

builder.Services.AddTransient<IValidationStrategy<UpdateCoffeeShopReviewCommand>, ReviewUpdateValidationStrategy>();

// Mapster
builder.Services.AddSingleton(MapsterConfiguration.CreateMapper());

// Yandex Geocoding Service
var yandexOptions = builder.Services.AddValidateOptions<YandexApiOptions>();
builder.Services.AddHttpClient<IYandexGeocodingService, YandexGeocodingService>(client =>
{
    client.BaseAddress = new Uri(yandexOptions.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(yandexOptions.TimeoutSeconds);
}).AddResiliencePolicies(nameof(YandexGeocodingService));

// MediatR
builder.Services.AddMediatRModule(typeof(GetAllModerationShopsHandler));

// Authorization policies (JWT validation happens in Gateway)
builder.Services.AddHeaderUserContext();
builder.Services.AddAuthorizationBuilder()
    .AddPolicy(RoleConsts.Admin, policy => policy.RequireRole(RoleConsts.Admin))
    .AddPolicy(RoleConsts.Owner, policy => policy.RequireRole(RoleConsts.Owner))
    .AddPolicy(RoleConsts.User, policy => policy.RequireRole(RoleConsts.User))
    .AddPolicy(RoleConsts.Moderator, policy => policy.RequireRole(RoleConsts.Moderator))
    .AddPolicy(RoleConsts.Employee, policy => policy.RequireRole(RoleConsts.Employee))
    .AddPolicy(RoleConsts.Roaster, policy => policy.RequireRole(RoleConsts.Roaster));

// CAP for event publishing and consuming
builder.Services.AddScoped<ModerationShopApproveCompleteHandler>();
builder.Services.AddScoped<CheckInCreatedConsumer>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    await app.ApplyMigrations<ModerationDbContext>();
    //await CoffeePeek.Moderation.Infrastructure.ModerationDbContext.SeedAsync(app.Services);
}

app.MapDefaultEndpoints();

app.UseSwaggerDocumentation();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
