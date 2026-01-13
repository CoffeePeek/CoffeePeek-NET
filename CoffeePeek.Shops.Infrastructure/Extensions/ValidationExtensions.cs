using CoffeePeek.Shared.Validation;
using CoffeePeek.Shops.Application.Commands.CoffeeShop;
using CoffeePeek.Shops.Application.Features.CoffeeShop.CheckIn;
using CoffeePeek.Shops.Application.Features.CoffeeShop.CreateCoffeeShopReview;
using CoffeePeek.Shops.Application.Features.CoffeeShop.GetCoffeeShops;
using CoffeePeek.Shops.Application.Features.Favorite.AddToFavorite;
using CoffeePeek.Shops.Application.Features.Favorite.RemoveFromFavorite;
using CoffeePeek.Shops.Application.Features.Review.UpdateCoffeeShopReview;
using CoffeePeek.Shops.Application.Services;
using CoffeePeek.Shops.Application.ValidationStrategy.CheckIn;
using CoffeePeek.Shops.Infrastructure.ValidationStrategy;
using CoffeePeek.Shops.Infrastructure.ValidationStrategy.CoffeeShop;
using CoffeePeek.Shops.Infrastructure.ValidationStrategy.Review;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeePeek.Shops.Infrastructure.Extensions;

public static class ValidationExtensions
{
    public static IServiceCollection AddValidators(this IServiceCollection services)
    {
        services.AddTransient<IValidationStrategy<CreateCoffeeShopReviewCommand>, CreateCoffeeShopReviewValidationStrategy>();
        services.AddTransient<IValidationStrategy<UpdateCoffeeShopReviewRequest>, ReviewUpdateValidationStrategy>();
        services.AddTransient<IValidationStrategy<GetCoffeeShopsQuery>, GetCoffeeShopsValidationStrategy>();
        services.AddTransient<IValidationStrategy<AddToFavoriteCommand>, AddToFavoriteValidationStrategy>();
        services.AddTransient<IValidationStrategy<RemoveFromFavoriteCommand>, RemoveFromFavoriteValidationStrategy>();
        services.AddTransient<IValidationStrategy<CreateCheckInRequest>, CheckInValidationStrategy>();
        
        return services;
    }
}