using CoffeePeek.Contract.Dtos.CoffeeShop;
using Mapster;
using ModerationReview = CoffeePeek.Moderation.Domain.Aggregates.ModerationReviewAggregate.ModerationReview;

namespace CoffeePeek.Moderation.Application.Mapper;

public partial class MapsterConfiguration
{
    private static void ConfigureModerationReview(TypeAdapterConfig config)
    {
        config.NewConfig<ModerationReview, ModerationReviewDto>();
    }
}