using System.Reflection;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeePeek.BuildingBlocks.Extensions;

public static class MapperExtensions
{
    public static IServiceCollection ConfigureMapster(this IServiceCollection services)
    {
        var typeAdapterConfig = TypeAdapterConfig.GlobalSettings;
        typeAdapterConfig.Scan(Assembly.GetExecutingAssembly());
        
        var mapperConfig = new MapsterMapper.Mapper(typeAdapterConfig);
        
        services.AddSingleton<IMapper>(mapperConfig);
        
        return services;
    }
}