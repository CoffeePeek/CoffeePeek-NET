using CoffeePeek.Contract.Dtos.Internal;

namespace CoffeePeek.Shops.Services.Interfaces;

public interface ICacheService
{
    Task<List<CityDto>> GetCities();
}