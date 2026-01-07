using CoffeePeek.Contract.Dtos;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Shops.Domain.Entities;
using Mapster;
using MapsterMapper;

namespace CoffeePeek.Shops.Application.Mapper;

public static class MapsterConfiguration
{
    public static IMapper CreateMapper()
    {
        var config = Configure();
        return new MapsterMapper.Mapper(config);
    }

    private static TypeAdapterConfig Configure()
    {
        var config = new TypeAdapterConfig();

        config.NewConfig<Shop, ShortShopDto>()
            .Map(dest => dest.Photos, src => src.ShopPhotos)
            .Map(dest => dest.Rating, src => src.Reviews.Any()
                ? src.Reviews.Average(r => (r.RatingCoffee + r.RatingPlace + r.RatingService) / 3m)
                : 0m)
            .Map(dest => dest.ReviewCount, src => src.Reviews.Count)
            .Map(dest => dest.IsOpen, src => true) //TODO fix
            .Map(dest => dest.Equipments, src => src.ShopEquipments.Select(x => x.Equipment));


        config.NewConfig<ShopPhoto, ShortPhotoMetadataDto>()
            .Map(dest => dest.FullUrl,
                src => $"https://bucket-dev-771f.up.railway.app/coffee.shops/{src.StorageKey}");
            
        config.NewConfig<Shop, ShopDto>()
            .Map(dest => dest.Photos, src => src.ShopPhotos)
            .Map(dest => dest.Rating, src => src.Reviews.Any()
                ? src.Reviews.Average(r => (r.RatingCoffee + r.RatingPlace + r.RatingService) / 3m)
                : 0m)
            .Map(dest => dest.ReviewCount, src => src.Reviews.Count)
            .Map(dest => dest.IsOpen, src => true) //TODO fix
            .Map(dest => dest.Beans, src => src.CoffeeBeanShops.Select(x => x.CoffeeBean))
            .Map(dest => dest.Roasters, src => src.RoasterShops.Select(x => x.Roaster))
            .Map(dest => dest.BrewMethods, src => src.ShopBrewMethods.Select(x => x.BrewMethod))
            .Map(dest => dest.Equipments, src => src.ShopEquipments.Select(x => x.Equipment));
        
        config.NewConfig<CheckIn, CheckInDto>()
            .Map(dest => dest.ShopName, src => src.Shop.Name);

        config.NewConfig<Review, ReviewDto>()
            .Map(dest => dest.UserId, src => src.UserId.GetHashCode())
            .Map(dest => dest.CreatedAt, src => src.ReviewDate)
            .Map(dest => dest.ShopName, src => src.Shop.Name);

        return config;
    }
}

