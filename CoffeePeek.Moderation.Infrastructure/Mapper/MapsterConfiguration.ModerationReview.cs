using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Moderation.Domain.Entities.ModerationReviewAggregate;
using Mapster;

namespace CoffeePeek.Moderation.Infrastructure.Mapper;

public partial class MapsterConfiguration
{
    private static void ConfigureModerationReview(TypeAdapterConfig config)
    {
        config.NewConfig<ModerationReview, ModerationReviewDto>();
    }
}