using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.Shops.Application.Services;
using CoffeePeek.Shops.Infrastructure.ValidationStrategy;
using CoffeePeek.Shops.Infrastructure.ValidationStrategy.CheckIn;
using CoffeePeek.Shops.Infrastructure.ValidationStrategy.CoffeeShop;
using CoffeePeek.Shops.Infrastructure.ValidationStrategy.Review;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeePeek.Shops.Infrastructure.Extensions;

public static class ValidationExtensions
{
    public static IServiceCollection AddValidators(this IServiceCollection services)
    {
        services.AddTransient<IValidationStrategy<AddCoffeeShopReviewRequest>, ReviewCreateValidationStrategy>();
        services.AddTransient<IValidationStrategy<UpdateCoffeeShopReviewRequest>, ReviewUpdateValidationStrategy>();
        services.AddTransient<IValidationStrategy<GetCoffeeShopsCommand>, GetCoffeeShopsValidationStrategy>();
        services.AddTransient<IValidationStrategy<AddToFavoriteCommand>, AddToFavoriteValidationStrategy>();
        services.AddTransient<IValidationStrategy<RemoveFromFavoriteCommand>, RemoveFromFavoriteValidationStrategy>();
        services.AddTransient<IValidationStrategy<CreateCheckInRequest>, CheckInValidationStrategy>();
        
        return services;
    }
}