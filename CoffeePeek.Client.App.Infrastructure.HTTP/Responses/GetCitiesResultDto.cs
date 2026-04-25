using CoffeePeek.Contract.Dtos.Internal;

namespace CoffeePeek.Client.App.Infrastructure.HTTP.Responses;

public sealed class GetCitiesResultDto
{
    public CityDto[] Cities { get; set; } = [];
}
