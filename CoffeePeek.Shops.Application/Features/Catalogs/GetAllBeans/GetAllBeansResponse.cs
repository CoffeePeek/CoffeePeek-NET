using CoffeePeek.Contract.Dtos.Shop;

namespace CoffeePeek.Shops.Application.Features.Catalogs.GetAllBeans;

public class GetAllBeansResponse(BeansDto[] beans)
{
    public BeansDto[] Beans { get; init; } = beans;
}