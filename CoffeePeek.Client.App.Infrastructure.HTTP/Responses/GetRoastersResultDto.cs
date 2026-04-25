using CoffeePeek.Contract.Dtos.Shop;

namespace CoffeePeek.Client.App.Infrastructure.HTTP.Responses;

public sealed class GetRoastersResultDto
{
    public RoasterDto[] Roasters { get; init; } = [];
}
