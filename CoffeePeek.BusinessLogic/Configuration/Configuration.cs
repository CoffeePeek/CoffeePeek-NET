using System.Reflection;
using CoffeePeek.BusinessLogic.RequestHandlers.CoffeeShop.Review.Behavior;
using CoffeePeek.Contract.Requests.CoffeeShop.Review;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.CoffeeShop.Review;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeePeek.BusinessLogic.Configuration;

public static class Configuration
{
    public static IServiceCollection ConfigureBusinessLogic(this IServiceCollection service)
    {
        ConfigureMediatR(service);
        
        return service;
    }

    private static void ConfigureMediatR(IServiceCollection service)
    {
        service.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });


        service.AddTransient<IPipelineBehavior<UpdateReviewCoffeeShopRequest, Response<UpdateReviewCoffeeShopResponse>>,
            UpdateReviewCoffeeShopBehavior>();
    }
}