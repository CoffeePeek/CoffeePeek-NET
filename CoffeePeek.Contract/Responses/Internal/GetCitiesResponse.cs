using CoffeePeek.Contract.Dtos.Internal;

namespace CoffeePeek.Contract.Response.Internal;

public class GetCitiesResponse(CityDto[] cities)
{
    public CityDto[] Cities { get; set; } = cities;
}