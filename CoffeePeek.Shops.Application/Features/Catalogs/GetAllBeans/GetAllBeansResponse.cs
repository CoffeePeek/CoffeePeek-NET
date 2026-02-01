using CoffeePeek.Contract.Dtos.Shop;

namespace CoffeePeek.Shops.Application.Features.Internal.GetAllBeans;

public class GetAllBeansResponse(BeansDto[] beans)
{
    public BeansDto[] Beans { get; init; } = beans;
}