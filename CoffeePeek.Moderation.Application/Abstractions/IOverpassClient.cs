using CoffeePeek.Moderation.Domain.Aggregates.ShopImportCandidateAggregate;

namespace CoffeePeek.Moderation.Application.Abstractions;

public interface IOverpassClient
{
    Task<IReadOnlyList<OsmCandidateSnapshot>> FetchMinskCafesAsync(CancellationToken ct = default);
}
