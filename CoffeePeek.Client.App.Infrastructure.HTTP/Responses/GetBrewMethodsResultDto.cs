using CoffeePeek.Contract.Dtos.Shop;

namespace CoffeePeek.Client.App.Infrastructure.HTTP.Responses;

public sealed class GetBrewMethodsResultDto
{
    public BrewMethodDto[] BrewMethods { get; init; } = [];
}
