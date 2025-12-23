using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Moderation.Domain.Entities;
using Mapster;
using MapsterMapper;

namespace CoffeePeek.Moderation.Application.Mapper;

public class MapsterConfiguration
{
    public static IMapper CreateMapper()
    {
        var config = Configure();
        return new MapsterMapper.Mapper(config);
    }

    private static TypeAdapterConfig Configure()
    {
        var config = new TypeAdapterConfig();

        config.NewConfig<ModerationShop, ModerationShopDto>()
            .Map(dest => dest.ShopContact, src => src.ModerationShopContact)
            .Map(dest => dest.ShopPhotos, src => src.ShopPhotos.Select(p => p.StorageKey).ToList())
            .Map(d => d.EquipmentIds, s => s.ModerationShopEquipments.Select(x => x.Id))
            .Map(d => d.CoffeeBeanIds, s => s.ModerationCoffeeBeanShops.Select(x => x.Id))
            .Map(d => d.RoasterIds, s => s.ModerationRoasterShops.Select(x => x.Id))
            .Map(d => d.BrewMethodIds, s => s.ModerationShopBrewMethods.Select(x => x.Id));

        config.NewConfig<ModerationShop, ShopDto>()
            .Map(d => d.ImageUrls, s => s.ShopPhotos.Select(x => x.StorageKey))
            .Map(d => d.Rating, s => 0)
            .Map(d => d.ReviewCount, s => 0)
            .Map(d => d.IsOpen, s => true)
            .Map(d => d.PriceRange, s => true)
            .Map(d => d.Location, s => s.Location)
            .Map(d => d.Beans, s => s.ModerationCoffeeBeanShops)
            .Map(d => d.Roasters, s => s.ModerationRoasterShops)
            .Map(d => d.Equipments, s => s.ModerationShopEquipments)
            .Map(d => d.BrewMethods, s => s.ModerationShopBrewMethods)
            .Map(d => d.ShopContact, s => s.ModerationShopContact)
            .Map(d => d.Schedules, s => s.Schedules);
        
        return config;
    }
}