using CoffeePeek.Shared.Validation;
using CoffeePeek.Shops.Application.Features.CheckIn.CreateCheckIn;
using CoffeePeek.Shops.Application.Features.Favorite.AddToFavorite;
using CoffeePeek.Shops.Application.Features.Favorite.RemoveFromFavorite;
using CoffeePeek.Shops.Application.ValidationStrategy.CheckIn;
using CoffeePeek.Shops.Application.ValidationStrategy.CoffeeShop;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeePeek.Shops.Infrastructure.Extensions;

public static class ValidationExtensions
{
    public static IServiceCollection AddValidators(this IServiceCollection services)
    {
        services.AddTransient<IValidationStrategy<AddToFavoriteCommand>, AddToFavoriteValidationStrategy>();
        services.AddTransient<IValidationStrategy<RemoveFromFavoriteCommand>, RemoveFromFavoriteValidationStrategy>();
        services.AddTransient<IAsyncValidationStrategy<CreateCheckInCommand>, CheckInValidationStrategy>();
        
        return services;
    }
}