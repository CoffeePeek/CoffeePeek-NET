using System.Net;
using System.Text.Json;
using CoffeePeek.Contract.Dtos.Import;
using CoffeePeek.Contract.Events.Moderation;
using CoffeePeek.Moderation.Domain.Aggregates.ShopImportCandidateAggregate;
using CoffeePeek.Moderation.Domain.Import;
using CoffeePeek.Shared.Domain.Places;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Response;

namespace CoffeePeek.Moderation.Application.Features.Import.IngestImportFile;

public record IngestImportFileCommand(JsonElement Body);

public static class IngestImportFileHandler
{
    public static async Task<(Response<IngestImportFileResultDto>, object?)> Handle(
        IngestImportFileCommand command,
        IShopImportCandidateRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        if (command.Body.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
        {
            return (Response<IngestImportFileResultDto>.Error(
                HttpStatusCode.BadRequest,
                "JSON body is required."), null);
        }

        if (ImportFileParser.LooksLikeDecisionsFile(command.Body))
        {
            return (Response<IngestImportFileResultDto>.Error(
                HttpStatusCode.BadRequest,
                "This looks like import-decisions.json. POST /api/admin/import/decisions instead."), null);
        }

        var parsed = ImportFileParser.Parse(command.Body);
        if (parsed.Count == 0)
        {
            return (Response<IngestImportFileResultDto>.Error(
                HttpStatusCode.BadRequest,
                "No coffee shops found in the JSON. Expected OSM candidates, GeoJSON, 2GIS/Google/Yandex places, or an array of {name, lat, lon}."),
                null);
        }

        var existing = await repository.ListAllAsync(ct);
        var now = DateTimeOffset.UtcNow;
        var inserted = 0;
        var enriched = 0;
        var unchanged = 0;
        var invalid = 0;
        var enrichItems = new List<ImportShopEnrichmentItem>();

        foreach (var place in parsed)
        {
            if (place.Snapshot.Latitude is < -90 or > 90 || place.Snapshot.Longitude is < -180 or > 180)
            {
                invalid++;
                continue;
            }

            var match = FindMatch(existing, place);
            if (match is not null)
            {
                if (match.EnrichFrom(place.Snapshot, now, place.GoogleMapsUri))
                    enriched++;
                else
                    unchanged++;

                AddEnrichment(enrichItems, match, place);
                continue;
            }

            var candidate = ShopImportCandidate.FromPlace(place.Source, place.Snapshot, now);
            if (!string.IsNullOrWhiteSpace(place.GoogleMapsUri))
                candidate.EnrichFrom(place.Snapshot, now, place.GoogleMapsUri);

            repository.Add(candidate);
            existing.Add(candidate);
            inserted++;
            AddEnrichment(enrichItems, candidate, place);
        }

        await unitOfWork.SaveChangesAsync(ct);

        object? outbound = enrichItems.Count == 0
            ? null
            : new ImportShopEnrichmentEvent(enrichItems);

        return (Response<IngestImportFileResultDto>.Success(
            new IngestImportFileResultDto(parsed.Count, inserted, enriched, unchanged, invalid)), outbound);
    }

    private static ShopImportCandidate? FindMatch(
        IReadOnlyList<ShopImportCandidate> existing,
        ParsedImportPlace place)
    {
        foreach (var candidate in existing)
        {
            if (string.Equals(candidate.ExternalId, place.Snapshot.ExternalId, StringComparison.Ordinal))
                return candidate;
        }

        foreach (var candidate in existing)
        {
            if (candidate.IsSamePlaceAs(place.Snapshot))
                return candidate;
        }

        return null;
    }

    private static void AddEnrichment(
        List<ImportShopEnrichmentItem> items,
        ShopImportCandidate candidate,
        ParsedImportPlace place)
    {
        var name = candidate.HasRealName
            ? candidate.Name!.Trim()
            : place.Snapshot.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name))
            return;

        items.Add(new ImportShopEnrichmentItem(
            candidate.ResultingShopId,
            name,
            ShopPlaceMatcher.PreferRicherText(candidate.Address, place.Snapshot.Address),
            candidate.Latitude,
            candidate.Longitude,
            candidate.Phone ?? place.Snapshot.Phone,
            candidate.Website ?? place.Snapshot.Website,
            candidate.Instagram ?? place.Snapshot.Instagram));
    }
}
