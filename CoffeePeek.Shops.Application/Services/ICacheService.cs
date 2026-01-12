using CoffeePeek.Contract.Dtos.Internal;
using CoffeePeek.Contract.Dtos.Shop;

namespace CoffeePeek.Shops.Application.Services;

public interface ICacheService
{
    Task<CityDto[]?> GetCities();
    Task<BeansDto[]?> GetBeans();
    Task<EquipmentDto[]?> GetEquipments();
    Task<RoasterDto[]?> GetRoasters();
    Task<BrewMethodDto[]?> GetBrewMethods();
    
    Task InvalidateShopDictionaries();
    Task InvalidateShopLists();
}