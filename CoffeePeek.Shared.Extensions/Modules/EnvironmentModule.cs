using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CoffeePeek.Shared.Extensions.Modules;

public static class EnvironmentModule
{
    public static WebApplicationBuilder ConfigureEnvironment(this WebApplicationBuilder builder)
    {
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

        return builder;
    }
}

