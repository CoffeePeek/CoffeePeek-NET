using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace CoffeePeek.Shared.Web.Logging;

public static class SerilogExtensions
{
    private const string DefaultTemplate =
        "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}";

    public static WebApplicationBuilder AddSerilogLogging(this WebApplicationBuilder builder)
    {
        var isDev = builder.Environment.IsDevelopment();
        var theme = isDev ? AnsiConsoleTheme.Code : ConsoleTheme.None;

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .MinimumLevel.Information()
            .WriteTo.Console(outputTemplate: DefaultTemplate, theme: theme)
            .CreateLogger();

        builder.Host.UseSerilog();

        return builder;
    }

    public static HostApplicationBuilder AddSerilogLogging(this HostApplicationBuilder builder)
    {
        var isDev = builder.Environment.IsDevelopment();
        var theme = isDev ? AnsiConsoleTheme.Code : ConsoleTheme.None;

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .MinimumLevel.Information()
            .WriteTo.Console(outputTemplate: DefaultTemplate, theme: theme)
            .CreateLogger();

        builder.Services.AddLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddSerilog();
        });

        return builder;
    }
}