using CoffeePeek.MediaService.BackgroundJobs;
using CoffeePeek.MediaService.Configuration;
using CoffeePeek.MediaService.Data;
using CoffeePeek.MediaService.Handlers;
using CoffeePeek.MediaService.Services;
using CoffeePeek.Shared.Extensions.Configuration;
using CoffeePeek.Shared.Extensions.Modules;
using CoffeePeek.Shared.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Minio;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add Swagger
builder.Services.AddSwaggerModule("CoffeePeek Media Service");

// Add authentication and authorization
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

// Add services
builder.Services.AddScoped<IStorageService, MinIOStorageService>();
builder.Services.AddScoped<IPhotoService, PhotoService>();
builder.Services.AddScoped<PhotoCleanupService>();

// Register CAP handlers
builder.Services.AddScoped<PhotoReplacedEventHandler>();

// Add background job
builder.Services.AddHostedService<PhotoCleanupBackgroundJob>();

// Configure MinIO
var minIoOptions = builder.Services.AddValidateOptions<MinIOOptions>();
builder.Services
    .AddMinio(configureClient =>
        configureClient
            .WithEndpoint(new Uri(minIoOptions.Endpoint))
            .WithCredentials(minIoOptions.AccessKey, minIoOptions.SecretKey)
            .Build()
    );


// Add DbContext and CAP Module
var postgresOptions = builder.Services.AddValidateOptions<PostgresCpOptions>();
builder.Services.AddDbContext<MediaDbContext>(options => options.UseNpgsql(postgresOptions.ConnectionString));
builder.Services.AddCapModule<MediaDbContext>(postgresOptions, AppResources.ShopsService);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Media Service API");
});

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
