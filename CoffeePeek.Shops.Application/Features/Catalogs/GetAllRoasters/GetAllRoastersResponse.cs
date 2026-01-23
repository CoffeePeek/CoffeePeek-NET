using CoffeePeek.Contract.Dtos.Shop;

namespace CoffeePeek.Shops.Application.Features.Internal.GetAllRoasters;

public record GetAllRoastersResponse(RoasterDto[] Roasters);