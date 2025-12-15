using CoffeePeek.Contract.Dtos.Shop;

namespace CoffeePeek.Contract.Responses.Internal;

public record GetAllRoastersResponse(RoasterDto[] Roasters)
{
    public RoasterDto[] Roasters { get; set; } = Roasters;
}