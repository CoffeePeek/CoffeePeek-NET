using CoffeePeek.Contract.Dtos.Internal;
using CoffeePeek.Contract.Dtos.Shop;

namespace CoffeePeek.ShopsService.Services.Interfaces;

public interface ICacheService
{
    Task<CityDto[]> GetCities();
    Task<BeansDto[]> GetBeans();
    Task<EquipmentDto[]> GetEquipments();
}