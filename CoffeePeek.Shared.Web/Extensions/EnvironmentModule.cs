using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace CoffeePeek.Shared.Web.Extensions;

public static class EnvironmentModule
{
    public static WebApplicationBuilder ConfigureEnvironment(this WebApplicationBuilder builder)
    {
        builder.WebHost.ConfigureEnvironment();
        
        return builder;
    }
    
    public static IWebHostBuilder ConfigureEnvironment(this IWebHostBuilder builder)
    {
        // Configure PORT from environment variable
        var port = Environment.GetEnvironmentVariable("PORT");
        if (!string.IsNullOrEmpty(port) && int.TryParse(port, out var portNumber))
        {
            builder.UseUrls($"http://*:{portNumber}");
        }

        return builder;
    }
}

