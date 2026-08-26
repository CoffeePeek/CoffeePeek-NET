using CoffeePeek.Moderation.Application.Abstractions;
using CoffeePeek.Moderation.Domain.Aggregates.ShopImportCandidateAggregate;
using CoffeePeek.Moderation.Domain.Import;

namespace CoffeePeek.Moderation.Infrastructure.Services;

public sealed class EmbeddedCoffeeMapCatalog : ICoffeeMapCatalog
{
    private readonly Lazy<IReadOnlyList<CoffeeMapCandidateSnapshot>> _cafes = new(Load);

    public Task<IReadOnlyList<CoffeeMapCandidateSnapshot>> GetCafesAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(_cafes.Value);
    }

    private static IReadOnlyList<CoffeeMapCandidateSnapshot> Load()
    {
        var assembly = typeof(EmbeddedCoffeeMapCatalog).Assembly;
        var resourceName = assembly.GetManifestResourceNames()
            .SingleOrDefault(name => name.EndsWith("coffeemap-cafes.json", StringComparison.Ordinal))
            ?? throw new InvalidOperationException("Embedded CoffeeMap catalog coffeemap-cafes.json was not found.");

        using var stream = assembly.GetManifestResourceStream(resourceName)
                           ?? throw new InvalidOperationException($"Could not open embedded resource {resourceName}.");
        using var reader = new StreamReader(stream);
        return CoffeeMapCafeParser.Parse(reader.ReadToEnd());
    }
}
