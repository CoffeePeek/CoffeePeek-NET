using CoffeePeek.Contract.Dtos.Internal;

namespace CoffeePeek.Shops.Application.Features.Catalogs.GetAllCities;

public class GetCitiesResponse(CityDto[] cities)
{
    public CityDto[] Cities { get; set; } = cities;
}