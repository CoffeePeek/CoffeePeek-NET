using CoffeePeek.Shared.Validation;
using CoffeePeek.Shops.Application.Features.CheckIn.CreateCheckIn;
using CoffeePeek.Shops.Application.ValidationStrategy.CheckIn;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeePeek.Shops.Application.Extensions;

public static class ValidationExtensions
{
    public static IServiceCollection AddValidators(this IServiceCollection services)
    {
        services.AddTransient<IAsyncValidationStrategy<CreateCheckInCommand>, CheckInValidationStrategy>();
        
        return services;
    }
}
