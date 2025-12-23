using CoffeePeek.Contract.Enums;
using CoffeePeek.Moderation.Domain.Entities;
using CoffeePeek.Shared.Infrastructure.Abstract;

namespace CoffeePeek.Moderation.Domain.Repositories;

public interface IModerationShopRepository
{
    Task<bool> Any(ISpecification<ModerationShop> spec, CancellationToken ct = default);
    Task<ModerationShop?> GetByIdWithDetails(Guid moderationShopId, CancellationToken ct = default);
    
    Task<ModerationShop?> GetByIdAsync(Guid id);
    Task<List<ModerationShop>> GetByUserIdAsync(Guid userId);
    Task<List<ModerationShop>> GetAllAsync();
    Task<List<ModerationShop>> GetByStatusAsync(ModerationStatus status);
    Task<ModerationShop?> GetByNameAndAddressAsync(string name, string address);
    Task<int> GetApprovedShopsCountByUserIdAsync(Guid userId);
    Task AddAsync(ModerationShop shop);
    Task UpdateAsync(ModerationShop shop);
}


