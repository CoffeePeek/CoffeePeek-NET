using System.Reflection;
using CoffeePeek.Data.Extensions;
using CoffeePeek.JobVacancies.Configuration;
using CoffeePeek.JobVacancies.Jobs;
using CoffeePeek.JobVacancies.Repository;
using CoffeePeek.JobVacancies.Services;
using CoffeePeek.Shared.Extensions.Configuration;
using CoffeePeek.Shared.Extensions.Middleware;
using CoffeePeek.Shared.Extensions.Modules;
using CoffeePeek.Shared.Extensions.Swagger;
using CoffeePeek.Shared.Infrastructure.Constants;
using CoffeePeek.Shared.Infrastructure.Options;
using Hangfire;
using Hangfire.PostgreSql;

var builder = WebApplication.CreateBuilder(args);

// Environment configuration
builder.ConfigureEnvironment();

// Controllers and API
builder.Services.AddControllersModule();

// Swagger
builder.Services.AddSwaggerModule("CoffeePeek Job vacancies API", "v1");

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
builder.Services.AddEfCoreData<JobVacanciesDbContext>(dbOptions);

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
        .UsePostgreSqlStorage(cpDbOptions.ConnectionString);
});
builder.Services.AddHangfireServer();

// MediatR
builder.Services.AddMediatRModule(Assembly.GetExecutingAssembly());

// Services

builder.Services.AddScoped<IJobVacancySyncService, JobVacancySyncService>();
builder.Services.AddScoped<ICitySyncService, CitySyncService>();

builder.Services.AddScoped<ICityRepository, CityRepository>();
builder.Services.AddScoped<IJobVacancyRepository, JobVacancyRepository>();

builder.Services.AddHttpClient<IHhApiService, HhApiService>();
builder.Services.AddHttpClient<IHhAuthService, HhAuthService>();

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