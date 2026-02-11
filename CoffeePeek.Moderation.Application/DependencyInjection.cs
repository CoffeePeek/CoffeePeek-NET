using CoffeePeek.Moderation.Application.Features.Review.SendReviewToModeration;
using CoffeePeek.Moderation.Application.Features.Review.UpdateCoffeeShopReview;
using CoffeePeek.Moderation.Application.Features.Shop.CreateShop;
using CoffeePeek.Moderation.Application.Mapper;
using CoffeePeek.Shared.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeePeek.Moderation.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Mapster
        services.AddSingleton(MapsterConfiguration.CreateMapper());

        // Validation
        services.AddTransient<IAsyncValidationStrategy<SendReviewToModerationCommand>, SendReviewToModerationValidationStrategy>();
        services.AddTransient<IValidationStrategy<UpdateCoffeeShopReviewCommand>, ReviewUpdateValidationStrategy>();

        // Application Services
        services.AddScoped<IModerationShopCreationService, ModerationShopCreationService>();

        return services;
    }
}
