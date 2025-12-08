using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Dtos.Shop;
using CoffeePeek.ShopsService.Entities;
using Mapster;
using MapsterMapper;

namespace CoffeePeek.ShopsService.Configuration;

public static class MapsterConfiguration
{
    public static IMapper CreateMapper()
    {
        var config = Configure();
        return new Mapper(config);
    }

    private static TypeAdapterConfig Configure()
    {
        var config = new TypeAdapterConfig();

        config.NewConfig<Shop, ShortShopDto>()
            .Map(dest => dest.ImageUrls, src => src.ShopPhotos.Select(x => x.Url))
            .Map(dest => dest.Rating, src => src.Reviews.Any()
                ? src.Reviews.Average(r => (r.RatingCoffee + r.RatingPlace + r.RatingService) / 3m)
                : 0m)
            .Map(dest => dest.ReviewCount, src => src.Reviews.Count)
            .Map(dest => dest.IsOpen, src => true) //TODO fix
            .Map(dest => dest.Equipments, src => src.ShopEquipments.Select(x => x.Equipment));

        config.NewConfig<Shop, ShopDto>()
            .Map(dest => dest.ImageUrls, src => src.ShopPhotos.Select(x => x.Url))
            .Map(dest => dest.Rating, src => src.Reviews.Any()
                ? src.Reviews.Average(r => (r.RatingCoffee + r.RatingPlace + r.RatingService) / 3m)
                : 0m)
            .Map(dest => dest.ReviewCount, src => src.Reviews.Count)
            .Map(dest => dest.IsOpen, src => true) //TODO fix
            .Map(dest => dest.Beans, src => src.CoffeeBeanShops)
            .Map(dest => dest.Roasters, src => src.RoasterShops)
            .Map(dest => dest.Equipments, src => src.ShopEquipments);

        // Маппинг для CoffeeBean -> BeansDto
        config.NewConfig<CoffeeBean, BeansDto>()
            .Map(dest => dest.Name, src => src.Name);

        // Маппинг для Equipment -> EquipmentDto
        config.NewConfig<Equipment, EquipmentDto>()
            .Map(dest => dest.Name, src => src.Name);
        
        return config;
    }
    
}

