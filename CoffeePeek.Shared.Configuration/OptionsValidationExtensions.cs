using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CoffeePeek.Shared.Configuration;

public static class OptionsValidationExtensions
{
    /// <summary>
    /// Bind options from configuration, validate data annotations, and validate on start.
    /// </summary>
    public static OptionsBuilder<TOptions> AddValidatedOptions<TOptions>(
        this IServiceCollection services,
        IConfigurationSection section)
        where TOptions : class, new()
    {
        return services
            .AddOptions<TOptions>()
            .Bind(section)
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }

    /// <summary>
    /// Register a custom validator for options.
    /// </summary>
    public static OptionsBuilder<TOptions> AddValidatedOptions<TOptions, TValidator>(
        this IServiceCollection services,
        IConfigurationSection section)
        where TOptions : class, new()
        where TValidator : class, IValidateOptions<TOptions>
    {
        services.AddSingleton<IValidateOptions<TOptions>, TValidator>();
        return services
            .AddOptions<TOptions>()
            .Bind(section)
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }
}

