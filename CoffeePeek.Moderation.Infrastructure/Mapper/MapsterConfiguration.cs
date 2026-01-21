using CoffeePeek.Contract.Dtos;
using CoffeePeek.Moderation.Domain.Entities;
using Mapster;
using MapsterMapper;

namespace CoffeePeek.Moderation.Infrastructure.Mapper;

public partial class MapsterConfiguration
{
    public static IMapper CreateMapper()
    {
        var config = Configure();
        return new MapsterMapper.Mapper(config);
    }

    private static TypeAdapterConfig Configure()
    {   
        var config = new TypeAdapterConfig();

        ConfigureModerationReview(config);
        ConfigureModerationShop(config);
        
        config.NewConfig<PhotoMetadata, ShortPhotoMetadataDto>()
            .Map(d => d.FullUrl, s => $"https://bucket-dev-771f.up.railway.app/coffee.shops/{s.StorageKey}");
        
        config.NewConfig<PhotoMetadata, PhotoMetadataDto>()
            .Map(d => d.FullUrl, s => $"https://bucket-dev-771f.up.railway.app/coffee.shops/{s.StorageKey}");
        
        return config;
    }
}