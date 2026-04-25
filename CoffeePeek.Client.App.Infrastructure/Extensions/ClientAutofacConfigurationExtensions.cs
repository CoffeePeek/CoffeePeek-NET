using Autofac;
using Microsoft.Extensions.Configuration;

namespace CoffeePeek.Client.App.Infrastructure.Extensions;

/// <summary>
/// Autofac equivalents of binding patterns used with <c>AddValidateOptions</c> in server projects:
/// section name defaults to the options type name (e.g. <c>ApiOptions</c> → <c>configuration["ApiOptions"]</c>).
/// </summary>
public static class ClientAutofacConfigurationExtensions
{
    public static IConfiguration RegisterClientAppConfiguration(
        this ContainerBuilder builder,
        string? basePath = null,
        string? environmentName = null)
    {
        basePath ??= AppContext.BaseDirectory;
        environmentName ??= Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
            ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? "Production";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: false)
            .Build();

        builder.RegisterInstance(configuration).As<IConfiguration>().SingleInstance();
        return configuration;
    }

    public static TOptions RegisterOptionsFromSection<TOptions>(
        this ContainerBuilder builder,
        IConfiguration configuration,
        string? sectionName = null)
        where TOptions : class, new()
    {
        sectionName ??= typeof(TOptions).Name;
        var options = configuration.GetSection(sectionName).Get<TOptions>() ?? new TOptions();
        builder.RegisterInstance(options).AsSelf().SingleInstance();
        return options;
    }
}
