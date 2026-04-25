using CoffeePeek.Contract.Dtos.Shop;

namespace CoffeePeek.Shops.Application.Features.Catalogs.GetAllBeans;

public class GetAllCoffeeBeansResponse(CoffeeBeansDto[] beans)
{
    public CoffeeBeansDto[] Beans { get; init; } = beans;
}