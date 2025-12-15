using CoffeePeek.Contract.Dtos.Internal;

namespace CoffeePeek.Contract.Responses.Internal;

public class GetCitiesResponse(CityDto[] cities)
{
    public CityDto[] Cities { get; set; } = cities;
}