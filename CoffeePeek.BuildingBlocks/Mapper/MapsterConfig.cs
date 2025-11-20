using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Dtos.User;
using CoffeePeek.Contract.Requests.CoffeeShop.Review;
using CoffeePeek.Domain.Entities.Review;
using CoffeePeek.Domain.Entities.Shop;
using CoffeePeek.Domain.Entities.Users;
using Mapster;

namespace CoffeePeek.BuildingBlocks.Mapper;

public class MapsterConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<SendCoffeeShopToModerationRequest, ModerationShop>()
            .Map(d => d.NotValidatedAddress, s => s.NotValidatedAddress);

        config.NewConfig<Review, CoffeeShopReviewDto>()
            .Map(dest => dest.ShopName, src => src.Shop != null ? src.Shop.Name : string.Empty);

        config.NewConfig<User, UserDto>()
            .Map(dest => dest.ReviewCount, src => src.Reviews != null ? src.Reviews.Count : 0);
    }
}