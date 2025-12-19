using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Enums;
using Coffeepeek.Moderation.Application.Commands;
using CoffeePeek.Moderation.Domain.Entities;
using Mapster;
using MapsterMapper;

namespace Coffeepeek.Moderation.Application.Mapper;

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
            .Map(dest => dest.ShopContact, src => src.ShopContacts)
            .Map(dest => dest.ShopPhotos, src => src.ShopPhotos.Select(p => p.Url).ToList())
            .Map(d => d.EquipmentIds, s => s.ModerationShopEquipments.Select(x => x.Id))
            .Map(d => d.CoffeeBeanIds, s => s.ModerationCoffeeBeanShops.Select(x => x.Id))
            .Map(d => d.RoasterIds, s => s.ModerationRoasterShops.Select(x => x.Id))
            .Map(d => d.BrewMethodIds, s => s.ModerationShopBrewMethods.Select(x => x.Id));

        config.NewConfig<ModerationShop, ShopDto>()
            .Map(d => d.ImageUrls, s => s.ShopPhotos.Select(x => x.Url))
            .Map(d => d.Rating, s => 0)
            .Map(d => d.ReviewCount, s => 0)
            .Map(d => d.IsOpen, s => true)
            .Map(d => d.PriceRange, s => true)
            .Map(d => d.Location, s => s.Location)
            .Map(d => d.Beans, s => s.ModerationCoffeeBeanShops)
            .Map(d => d.Roasters, s => s.ModerationRoasterShops)
            .Map(d => d.Equipments, s => s.ModerationShopEquipments)
            .Map(d => d.BrewMethods, s => s.ModerationShopBrewMethods)
            .Map(d => d.ShopContact, s => s.ShopContacts)
            .Map(d => d.Schedules, s => s.Schedules);
        
        //TODO: CP-107 remove its bad practics
        config.NewConfig<SendCoffeeShopToModerationRequest, ModerationShop>()
            .Map(d => d.Id, _ => Guid.NewGuid())
            .Map(d => d.NotValidatedAddress, s => s.NotValidatedAddress)
            .Map(d => d.PriceRange, s => s.PriceRange ?? PriceRange.Cheap)
            .Map(d => d.ModerationStatus, _ => ModerationStatus.Pending)
            .Map(d => d.Status, _ => ShopStatus.NotConfirmed);
        
        return config;
    }
}