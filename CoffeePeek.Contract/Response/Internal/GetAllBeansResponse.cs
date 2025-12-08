using CoffeePeek.Contract.Dtos.Shop;

namespace CoffeePeek.Contract.Response.Internal;

public class GetAllBeansResponse(BeansDto[] beans)
{
    public BeansDto[] Beans { get; set; } = beans;
}