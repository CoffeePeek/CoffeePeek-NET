using CoffeePeek.Domain.Enums.Shop;
using CoffeePeek.ModerationService.Models;

namespace CoffeePeek.ModerationService.Repositories;

public interface IModerationShopRepository
{
    Task<ModerationShop?> GetByIdAsync(int id);
    Task<List<ModerationShop>> GetByUserIdAsync(Guid userId);
    Task<List<ModerationShop>> GetAllAsync();
    Task<List<ModerationShop>> GetByStatusAsync(ModerationStatus status);
    Task<ModerationShop?> GetByNameAndAddressAsync(string name, string address, Guid userId);
    Task AddAsync(ModerationShop shop);
    Task UpdateAsync(ModerationShop shop);
    Task SaveChangesAsync();
}

