using CoffeePeek.Contract.Requests.CoffeeShop.Review;
using CoffeePeek.Domain.Entities.Shop;
using Mapster;

namespace CoffeePeek.Contract.Mapper;

public class MapsterConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<SendCoffeeShopToReviewRequest, ReviewShop>()
            .Map(d => d.NotValidatedAddress, s => s.NotValidatedAddress);
    }
}