using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CoffeePeek.Shared.Extensions.Configuration;

public static class ConfigurationExtensions
{
    extension(IServiceCollection service)
    {
        public TModel AddValidateOptions<TModel>() where TModel : class, new()
        {
            service.AddOptions<TModel>()
                .BindConfiguration(typeof(TModel).Name)
                .ValidateDataAnnotations();
            var options = service.BuildServiceProvider().GetRequiredService<IOptions<TModel>>().Value;
            service.AddSingleton(options);
        
            return options; 
        }

        public TModel GetOptions<TModel>() where TModel : new()
        {
            var options = service.BuildServiceProvider().GetService<TModel>();
        
            return options ?? new TModel();
        }
    }
}