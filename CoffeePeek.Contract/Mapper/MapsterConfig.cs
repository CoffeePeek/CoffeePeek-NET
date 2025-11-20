using CoffeePeek.Contract.Requests.CoffeeShop.Review;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Domain.Entities.Shop;
using CoffeePeek.Domain.Entities.Review;
using Mapster;

namespace CoffeePeek.Contract.Mapper;

public class MapsterConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<SendCoffeeShopToReviewRequest, ReviewShop>()
            .Map(d => d.NotValidatedAddress, s => s.NotValidatedAddress);

        config.NewConfig<Review, CoffeeShopReviewDto>()
            .Map(dest => dest.ShopName, src => src.Shop != null ? src.Shop.Name : string.Empty);
    }
}