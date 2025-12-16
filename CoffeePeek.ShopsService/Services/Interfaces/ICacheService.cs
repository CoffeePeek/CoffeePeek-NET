using CoffeePeek.Contract.Dtos.Internal;
using CoffeePeek.Contract.Dtos.Shop;

namespace CoffeePeek.ShopsService.Services.Interfaces;

public interface ICacheService
{
    Task<CityDto[]?> GetCities();
    Task<BeansDto[]?> GetBeans();
    Task<EquipmentDto[]?> GetEquipments();
    Task<RoasterDto[]?> GetRoasters();
    Task<BrewMethodDto[]?> GetBrewMethods();
    
    Task InvalidateShopDictionaries(CancellationToken cancellationToken = default);
    Task InvalidateShopLists(CancellationToken cancellationToken = default);
}