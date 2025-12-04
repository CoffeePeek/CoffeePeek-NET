using CoffeePeek.Contract.Dtos.Internal;

namespace CoffeePeek.ShopsService.Services.Interfaces;

public interface ICacheService
{
    Task<CityDto[]> GetCities();
}