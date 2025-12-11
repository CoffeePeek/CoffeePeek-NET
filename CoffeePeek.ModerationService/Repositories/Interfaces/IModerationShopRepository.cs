using CoffeePeek.Contract.Enums;
using CoffeePeek.ModerationService.Models;

namespace CoffeePeek.ModerationService.Repositories.Interfaces;

public interface IModerationShopRepository
{
    Task<ModerationShop?> GetByIdAsync(Guid id);
    Task<List<ModerationShop>> GetByUserIdAsync(Guid userId);
    Task<List<ModerationShop>> GetAllAsync();
    Task<List<ModerationShop>> GetByStatusAsync(ModerationStatus status);
    Task<ModerationShop?> GetByNameAndAddressAsync(string name, string address, Guid userId);
    Task<int> GetApprovedShopsCountByUserIdAsync(Guid userId);
    Task AddAsync(ModerationShop shop);
    Task UpdateAsync(ModerationShop shop);
}


