using CoffeePeek.Contract.Abstract;
using CoffeePeek.JobVacancies.Application.Handlers;
using CoffeePeek.JobVacancies.Application.Repositories;
using CoffeePeek.JobVacancies.Application.Services;
using CoffeePeek.JobVacancies.Configuration;
using CoffeePeek.JobVacancies.Domain.Entities;
using CoffeePeek.JobVacancies.Domain.Repositories;
using CoffeePeek.JobVacancies.Infrastructure;
using CoffeePeek.JobVacancies.Infrastructure.Configuration;
using CoffeePeek.JobVacancies.Infrastructure.Services;
using CoffeePeek.JobVacancies.Jobs;
using CoffeePeek.Shared.Extensions.Configuration;
using CoffeePeek.Shared.Extensions.Handlers;
using CoffeePeek.Shared.Extensions.Modules;
using CoffeePeek.Shared.Extensions.Resilience;
using CoffeePeek.Shared.Extensions.Swagger;
using CoffeePeek.Shared.Infrastructure.Constants;
using CoffeePeek.Shared.Infrastructure.Options;
using Hangfire;
using Hangfire.PostgreSql;
using CoffeePeek.Shared.Extensions.Logging;
using CoffePeek.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseSentry();

builder.AddServiceDefaults();

builder.AddSerilogLogging();

// Environment configuration
builder.ConfigureEnvironment();

// Controllers and API
builder.Services.AddControllersModule();

// Swagger
builder.Services.AddSwaggerModule("CoffeePeek Job vacancies API");

// Authentication & Authorization
builder.Services.AddJwtAuthModule();
builder.Services.AddValidateOptions<JWTOptions>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(RoleConsts.Admin, policy => policy.RequireRole(RoleConsts.Admin));
    options.AddPolicy(RoleConsts.User, policy => policy.RequireRole(RoleConsts.User));
});

// Database
var dbOptions = builder.Services.GetDatabaseOptions(builder.Configuration, databaseName: AppResources.JobVacanciesDb);
builder.Services.AddEfCoreData<JobVacanciesDbContext>(dbOptions);
builder.Services.AddGenericRepository<JobVacancy, JobVacanciesDbContext>();
builder.Services.AddGenericRepository<CityMap, JobVacanciesDbContext>();

// Config
builder.Services.Configure<HhApiOptions>(builder.Configuration.GetSection("HhApi"));

// Cache (Redis)
builder.Services.AddCacheModule();

// Mapster
builder.Services.AddSingleton(MapsterConfiguration.CreateMapper());

// Hangfire
builder.Services.AddHangfire((sp, config) =>
{
    var cpDbOptions = sp.GetRequiredService<PostgresCpOptions>();
    config
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(cpDbOptions.ConnectionString));
});
builder.Services.AddHangfireServer();

// MediatR
builder.Services.AddMediatRModule(typeof(GetVacanciesHandler));

// Services

builder.Services.AddScoped<IJobVacancySyncService, JobVacancySyncService>();
builder.Services.AddScoped<ICitySyncService, CitySyncService>();

builder.Services.AddScoped<ICityRepository, CityRepository>();
builder.Services.AddScoped<IJobVacancyRepository, JobVacancyRepository>();

builder.Services.AddHttpClient<IHhApiService, HhApiService>()
    .AddResiliencePolicies(nameof(HhApiService));
builder.Services.AddHttpClient<IHhAuthService, HhAuthService>()
    .AddResiliencePolicies(nameof(HhAuthService));

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();

app.MapDefaultEndpoints();

app.UseCors();

// Swagger documentation
app.UseSwaggerDocumentation();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.UseHangfireDashboard();

// Recurring job: sync cities with hh.ru areas once per 24 hours
RecurringJob.AddOrUpdate<CityAreaSyncJob>(
    "city-area-sync",
    job => job.ExecuteAsync(JobCancellationToken.Null),
    Cron.Daily);

RecurringJob.AddOrUpdate<HhVacanciesRecurringJob>(
    "vacancies-sync",
    job => job.ExecuteAsync(JobCancellationToken.Null),
    Cron.HourInterval(2));

app.Run();