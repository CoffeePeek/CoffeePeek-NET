using CoffeePeek.Contract.Dtos;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Shops.Domain.Entities;
using CoffeePeek.Shops.Domain.Entities.CoffeeShopAggregate;
using Mapster;
using Review = CoffeePeek.Shops.Domain.Entities.ReviewAggregate.Review;

namespace CoffeePeek.Shops.Application.Mapper;

public class MapsterConfiguration : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CoffeeShop, ShortShopDto>()
            .Map(dest => dest.Photos, src => src.ShopPhotos)
            .Map(dest => dest.ReviewCount, src => src.Reviews.Count)
            .Map(dest => dest.ShopContact, src => src.Contact)
            .Map(dest => dest.Beans, src => src.CoffeeBeans)
            
            .Ignore(dest => dest.IsFavorite)
            .Ignore(dest => dest.IsVisited);


        config.NewConfig<ShopPhoto, ShortPhotoMetadataDto>()
            .Map(dest => dest.FullUrl,
                src => $"https://bucket-dev-771f.up.railway.app/coffee.shops/{src.StorageKey}");


        config.NewConfig<CoffeeShop, ShopDto>()
            .Map(d => d.Photos, s => s.ShopPhotos)
            .Map(dest => dest.Rating, src => src.Reviews.Any()
                ? src.Reviews.Average(r => (r.RatingCoffee + r.RatingPlace + r.RatingService) / 3m)
                : 0m)
            .Map(dest => dest.ReviewCount, src => src.Reviews.Count)
            .Map(dest => dest.IsOpen, src => true)
            .Map(dest => dest.CoffeeBeans, src => src.CoffeeBeans);

        config.NewConfig<CoffeeShop, CoffeeShopDetailsDto>()
            .Map(d => d.Photos, s => s.ShopPhotos)
            .Map(dest => dest.Rating, src => src.Reviews.Any()
                ? src.Reviews.Average(r => (r.RatingCoffee + r.RatingPlace + r.RatingService) / 3m)
                : 0m)
            .Map(dest => dest.ReviewCount, src => src.Reviews.Count)
            .Map(dest => dest.IsOpen, src => true);
        
        config.NewConfig<CheckIn, CheckInDto>()
            .Map(dest => dest.ShopName, src => src.CoffeeShop.Name);

        config.NewConfig<Review, ReviewDto>()
            .Map(dest => dest.CreatedAt, src => src.ReviewDate)
            .Map(dest => dest.ShopName, src => src.Shop.Name);
    }
}

