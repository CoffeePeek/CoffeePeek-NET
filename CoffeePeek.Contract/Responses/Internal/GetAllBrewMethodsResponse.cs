using CoffeePeek.Contract.Dtos.Shop;

namespace CoffeePeek.Contract.Responses.Internal;

public record GetAllBrewMethodsResponse(BrewMethodDto[] BrewMethods)
{
    public BrewMethodDto[] BrewMethods { get; set; } = BrewMethods;
}