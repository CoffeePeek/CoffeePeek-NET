using CoffeePeek.Moderation.Domain.Aggregates.ShopImportCandidateAggregate;

namespace CoffeePeek.Moderation.Application.Abstractions;

public interface ICoffeeMapCatalog
{
    Task<IReadOnlyList<CoffeeMapCandidateSnapshot>> GetCafesAsync(CancellationToken ct = default);
}
