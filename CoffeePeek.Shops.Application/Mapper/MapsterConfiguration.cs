using CoffeePeek.Contract.Dtos;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Dtos.Shop;
using CoffeePeek.Contract.Enums;
using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;
using CoffeePeek.Shops.Domain.Entities;
using Mapster;
using CheckIn = CoffeePeek.Shops.Domain.Aggregates.CheckInAggregate.CheckIn;
using Review = CoffeePeek.Shops.Domain.Aggregates.ReviewAggregate.Review;

namespace CoffeePeek.Shops.Application.Mapper;

public class MapsterConfiguration : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CoffeeShop, ShortShopDto>()
            .Map(dest => dest.CityId, src => src.Location.CityId)
            .Map(dest => dest.Photos, src => src.ShopPhotos)
            .Map(dest => dest.ShopContact, src => src.Contact)
            .Map(dest => dest.Beans, src => src.CoffeeBeans)
            // IsOpen uses Schedules which can't be translated to SQL — mark as not open; set in handler if needed
            .Map(dest => dest.IsOpen, src => false)
            // Rating and ReviewCount are set manually in handlers via repository
            .Ignore(dest => dest.Rating)
            .Ignore(dest => dest.ReviewCount)
            .Ignore(dest => dest.IsFavorite)
            .Ignore(dest => dest.IsVisited);

        config.NewConfig<ShopPhoto, ShortPhotoMetadataDto>()
            .Map(dest => dest.FullUrl,
                src => $"https://bucket-dev-771f.up.railway.app/coffee.shops/{src.StorageKey}");

        config.NewConfig<CoffeeShop, ShopDto>()
            .Map(d => d.Photos, s => s.ShopPhotos)
            .Map(dest => dest.IsOpen, src => true)
            .Map(dest => dest.CoffeeBeans, src => src.CoffeeBeans)
            // Rating, ReviewCount and Reviews are set manually in handlers via repository
            .Ignore(dest => dest.Rating)
            .Ignore(dest => dest.ReviewCount)
            .Ignore(dest => dest.Reviews);

        config.NewConfig<CoffeeShop, CoffeeShopDetailsDto>()
            .Map(d => d.CityId, s => s.Location.CityId)
            .Map(d => d.Photos, s => s.ShopPhotos)
            .Map(d => d.ShopContact, s => s.Contact)
            .Map(d => d.Schedules, s => s.Schedules)
            .Map(dest => dest.IsOpen, src => src.IsOpen)
            .Map(dest => dest.IsNew, src => src.IsNew)
            // Rating, ReviewCount and Reviews are set manually in handlers via repository
            .Ignore(dest => dest.Rating)
            .Ignore(dest => dest.ReviewCount)
            .Ignore(dest => dest.Reviews);
        
        config.NewConfig<CheckIn, CheckInDto>()
            // ShopName is set manually in handlers via repository
            .Ignore(dest => dest.ShopName);
        
        config.NewConfig<Equipment, EquipmentDto>()
            .Map(dest => dest.Model, src => src.ModelName)
            .Map(dest => dest.Category, src => (EquipmentCategoryEnum)src.CategoryId);
    }
}

