using CoffeePeek.Photo.Api.Configuration;

namespace CoffeePeek.Photo.Api;

public class Program
{
    public static void Main(string[] args)
    {
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
        
        builder.Services.AddAuthorization();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddControllers();
        
        builder.Services.ConfigureApi();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // Use CORS if configured
        if (!string.IsNullOrEmpty(allowedOrigins))
        {
            app.UseCors();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();
        
        app.Run();
    }
}