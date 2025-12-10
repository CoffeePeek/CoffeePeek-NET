using CoffeePeek.Contract.Dtos.Shop;

namespace CoffeePeek.Contract.Responses.Internal;

public class GetAllBeansResponse(BeansDto[] beans)
{
    public BeansDto[] Beans { get; init; } = beans;
}