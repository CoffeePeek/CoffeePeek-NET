using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Dtos.Shop;
using CoffeePeek.Moderation.Domain.Entities;
using Mapster;

namespace CoffeePeek.Moderation.Infrastructure.Mapper;

public partial class MapsterConfiguration
{
    private static void ConfigureModerationShop(TypeAdapterConfig config)
    {
        config.NewConfig<ModerationShop, ModerationShopDto>()
            .Map(dest => dest.Address, src => src.Location.Address)
            .Map(dest => dest.AddressIsValidated, src => src.Location.IsAddressValidated)
            .Map(dest => dest.ShopPhotos, src => src.ShopPhotos)
            .Map(dest => dest.ShopContact, src => src.Contact)
            .Map(d => d.EquipmentIds, s => s.ModerationShopEquipments.Select(x => x.EquipmentId))
            .Map(d => d.CoffeeBeanIds, s => s.ModerationCoffeeBeanShops.Select(x => x.CoffeeBeanId))
            .Map(d => d.RoasterIds, s => s.ModerationRoasterShops.Select(x => x.RoasterId))
            .Map(d => d.BrewMethodIds, s => s.ModerationShopBrewMethods.Select(x => x.BrewMethodId));
        
        config.NewConfig<ModerationShop, ShopDto>()
            .Map(d => d.Photos, s => s.ShopPhotos)
            .Map(d => d.Rating, s => 0)
            .Map(d => d.ReviewCount, s => 0)
            .Map(d => d.IsOpen, s => true)
            .Map(d => d.CoffeeBeans, s => s.ModerationCoffeeBeanShops)
            .Map(d => d.Roasters, s => s.ModerationRoasterShops)
            .Map(d => d.Equipments, s => s.ModerationShopEquipments)
            .Map(d => d.BrewMethods, s => s.ModerationShopBrewMethods)
            .Map(d => d.ShopContact, s => s.Contact)
            .Map(d => d.Schedules, s => s.Schedules);
        
        config.NewConfig<ModerationShopRoaster, RoasterDto>()
            .Map(dest => dest.Id, src => src.RoasterId);
        config.NewConfig<ModerationShopEquipment, EquipmentDto>()
            .Map(dest => dest.Id, src => src.EquipmentId);
        config.NewConfig<ModerationShopBrewMethod, BrewMethodDto>()
            .Map(dest => dest.Id, src => src.BrewMethodId);
        config.NewConfig<ModerationCoffeeBeanShop, BeansDto>()
            .Map(dest => dest.Id, src => src.CoffeeBeanId);
    }
}