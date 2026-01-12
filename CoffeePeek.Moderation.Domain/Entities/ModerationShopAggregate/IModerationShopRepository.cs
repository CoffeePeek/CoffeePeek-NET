using CoffeePeek.Moderation.Domain.Entities;
using CoffeePeek.Shared.Infrastructure.Abstract;

namespace CoffeePeek.Moderation.Domain.Repositories;

public interface IModerationShopRepository
{
    Task<bool> Any(ISpecification<ModerationShop> spec, CancellationToken ct = default);
    Task<ModerationShop?> GetByIdWithDetails(Guid moderationShopId, CancellationToken ct = default);
    Task<IReadOnlyList<ModerationShop>> GetAllForReviewAsync();
    
    Task<ModerationShop?> GetByIdAsync(Guid id);
    Task AddAsync(ModerationShop shop);
}


