using CoffeePeek.BusinessLogic.Abstractions;
using CoffeePeek.BusinessLogic.Abstractions.Review;
using CoffeePeek.Contract.Dtos.User;
using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.Shops.Abstractions;

namespace CoffeePeek.Shops.Extensions;

public static class ValidationExtensions
{
    public static IServiceCollection AddValidators(this IServiceCollection services)
    {
        services.AddTransient<IValidationStrategy<UserDto>, UserCreateValidationStrategy>();
        services.AddTransient<IValidationStrategy<AddCoffeeShopReviewRequest>, ReviewCreateValidationStrategy>();
        services.AddTransient<IValidationStrategy<UpdateCoffeeShopReviewRequest>, ReviewUpdateValidationStrategy>();
        
        return services;
    }
}