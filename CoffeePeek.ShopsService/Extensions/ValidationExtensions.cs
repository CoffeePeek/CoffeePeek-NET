using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.ShopsService.Abstractions.ValidationStrategy;
using CoffeePeek.ShopsService.Abstractions.ValidationStrategy.CoffeeShop;
using CoffeePeek.ShopsService.Abstractions.ValidationStrategy.Review;

namespace CoffeePeek.ShopsService.Extensions;

public static class ValidationExtensions
{
    public static IServiceCollection AddValidators(this IServiceCollection services)
    {
        services.AddTransient<IValidationStrategy<AddCoffeeShopReviewRequest>, ReviewCreateValidationStrategy>();
        services.AddTransient<IValidationStrategy<UpdateCoffeeShopReviewRequest>, ReviewUpdateValidationStrategy>();
        services.AddTransient<IValidationStrategy<GetCoffeeShopsCommand>, GetCoffeeShopsValidationStrategy>();
        services.AddTransient<IValidationStrategy<AddToFavoriteCommand>, AddToFavoriteValidationStrategy>();
        services.AddTransient<IValidationStrategy<RemoveFromFavoriteCommand>, RemoveFromFavoriteValidationStrategy>();
        
        return services;
    }
}