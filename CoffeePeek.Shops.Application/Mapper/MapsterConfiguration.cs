using CoffeePeek.Contract.Dtos;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Dtos.Shop;
using CoffeePeek.Contract.Enums;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Options;
using CoffeePeek.Shops.Domain.Aggregates.BrewMethods;
using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;
using CoffeePeek.Shops.Domain.Entities;
using Mapster;
using MapsterMapper;
using CheckIn = CoffeePeek.Shops.Domain.Aggregates.CheckInAggregate.CheckIn;

namespace CoffeePeek.Shops.Application.Mapper;

public static class MapsterConfiguration
{
    public static TypeAdapterConfig CreateConfig(MediaPublicUrlOptions mediaOptions) => Configure(mediaOptions);

    public static IMapper CreateMapper(MediaPublicUrlOptions mediaOptions) =>
        new MapsterMapper.Mapper(Configure(mediaOptions));

    private static TypeAdapterConfig Configure(MediaPublicUrlOptions mediaOptions)
    {
        var config = new TypeAdapterConfig();

        config.NewConfig<CoffeeShop, ShortShopDto>()
            .Map(dest => dest.CityId, src => src.Location.CityId)
            .Map(dest => dest.Photos, src => src.ShopPhotos.OrderBy(p => p.SortIndex).ThenBy(p => p.CreatedAtUtc))
            .Map(dest => dest.ShopContact, src => src.Contact)
            .Map(dest => dest.Beans, src => src.CoffeeBeans)
            .Map(dest => dest.Type, src => (Contract.Enums.CoffeeShopType?)(int?)src.CoffeeFocus)
            // IsOpen/IsNew patched after materialize in CoffeeShopQueries (Schedules not in ProjectTo)
            .Ignore(dest => dest.IsOpen)
            .Ignore(dest => dest.IsNew)
            // Rating and ReviewCount are set manually in handlers via repository
            .Ignore(dest => dest.Rating)
            .Ignore(dest => dest.ReviewCount)
            .Ignore(dest => dest.IsVisited);

        config.NewConfig<ShopPhoto, ShortPhotoMetadataDto>()
            .Map(dest => dest.FullUrl, src =>
                MediaStorageUrlBuilder.BuildPublicUrl(
                    mediaOptions.PublicEndpoint,
                    mediaOptions.ShopBucketName,
                    src.StorageKey) ?? string.Empty);

        config.NewConfig<CoffeeShop, ShopDto>()
            .Map(d => d.Photos, s => s.ShopPhotos.OrderBy(p => p.SortIndex).ThenBy(p => p.CreatedAtUtc))
            .Map(dest => dest.IsOpen, src => true)
            .Map(dest => dest.Type, src => (Contract.Enums.CoffeeShopType?)(int?)src.CoffeeFocus)
            .Map(dest => dest.CoffeeBeans, src => src.CoffeeBeans)
            // Rating, ReviewCount and Reviews are set manually in handlers via repository
            .Ignore(dest => dest.Rating)
            .Ignore(dest => dest.ReviewCount)
            .Ignore(dest => dest.Reviews);

        config.NewConfig<CoffeeShop, CoffeeShopDetailsDto>()
            .Map(d => d.CityId, s => s.Location.CityId)
            .Map(d => d.Photos, s => s.ShopPhotos.OrderBy(p => p.SortIndex).ThenBy(p => p.CreatedAtUtc))
            .Map(d => d.ShopContact, s => s.Contact)
            .Map(d => d.Schedules, s => s.Schedules)
            // IsOpen/IsNew patched after materialize in CoffeeShopQueries (computed props are not ProjectTo-safe)
            .Ignore(dest => dest.IsOpen)
            .Ignore(dest => dest.IsNew)
            .Map(dest => dest.Type, src => (Contract.Enums.CoffeeShopType?)(int?)src.CoffeeFocus)
            // Tags loaded separately in CoffeeShopQueries.GetDetailsById
            .Ignore(dest => dest.Tags)
            .Ignore(dest => dest.Menu)
            // Rating, ReviewCount and Reviews are set manually in handlers via repository
            .Ignore(dest => dest.Rating)
            .Ignore(dest => dest.ReviewCount)
            .Ignore(dest => dest.Reviews);

        config.NewConfig<CheckIn, CheckInDto>()
            // ShopName is set manually in handlers via repository
            .Ignore(dest => dest.ShopName);

        config.NewConfig<EquipmentCategory, EquipmentCategoryEnum>()
            .MapWith(category => (EquipmentCategoryEnum)category.Id);

        config.NewConfig<Equipment, EquipmentDto>()
            .Map(dest => dest.Model, src => src.ModelName)
            .Map(dest => dest.Category, src => (EquipmentCategoryEnum)src.CategoryId);

        config.NewConfig<BrewMethod, BrewMethodDto>()
            .Map(dest => dest.Category, src => (BrewMethodCategoryEnum)(int)src.Category);

        return config;
    }
}
