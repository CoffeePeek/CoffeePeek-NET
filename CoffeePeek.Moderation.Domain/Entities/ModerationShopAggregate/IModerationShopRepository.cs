using CoffeePeek.Shared.Infrastructure.Abstract;

namespace CoffeePeek.Moderation.Domain.Entities;

public interface IModerationShopRepository
{
    Task<bool> Any(ISpecification<ModerationShop> spec, CancellationToken ct = default);
    Task<ModerationShop?> GetByIdWithOutDetails(Guid moderationShopId, CancellationToken ct = default);
    Task<IReadOnlyList<ModerationShop>> GetAllForReviewAsync();
    
    Task<ModerationShop?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(ModerationShop shop);
}


