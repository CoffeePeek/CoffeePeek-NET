using CoffeePeek.Contract.Dtos.Shop;

namespace CoffeePeek.Client.App.Infrastructure.HTTP.Responses;

public sealed class GetBeansResultDto
{
    public CoffeeBeansDto[] Beans { get; init; } = [];
}
