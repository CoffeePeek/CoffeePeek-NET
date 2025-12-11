using CoffeePeek.JobVacancies.Constants;
using CoffeePeek.JobVacancies.Entities;
using CoffeePeek.JobVacancies.Models.Responses;
using CoffeePeek.JobVacancies.Repository;

namespace CoffeePeek.JobVacancies.Services;

public class CitySyncService(ICityRepository cityRepository, ILogger<CitySyncService> logger) : ICitySyncService
{
    public async Task<(int updated, int notFound)> SyncCitiesAsync(List<HhAreaNode> hhAreas,
        CancellationToken ct = default)
    {
        var flatAreas = FlattenAreas(hhAreas);

        var updated = 0;
        var notFound = 0;

        foreach (var (cityId, cityName) in CitiesConsts.Cities)
        {
            var normalized = cityName.Trim().ToLowerInvariant();

            if (!flatAreas.TryGetValue(normalized, out var rawAreaId) || !int.TryParse(rawAreaId, out var hhAreaId))
            {
                logger.LogWarning("City not found or invalid area ID: {CityName} (Id={CityId})", cityName, cityId);
                notFound++;
                continue;
            }

            await cityRepository.UpsertAsync(new CityMap
            {
                CityId = cityId,
                CityName = cityName,
                HhAreaId = hhAreaId,
                UpdatedAt = DateTime.UtcNow
            }, ct);

            updated++;
            logger.LogDebug("Mapped: {CityName} -> {HhAreaId}", cityName, hhAreaId);
        }

        return (updated, notFound);
    }

    private static Dictionary<string, string> FlattenAreas(List<HhAreaNode> nodes)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var node in nodes)
            Traverse(node);

        return map;

        void Traverse(HhAreaNode node)
        {
            if (!string.IsNullOrWhiteSpace(node.Name) && !string.IsNullOrWhiteSpace(node.Id))
                map[node.Name.Trim().ToLowerInvariant()] = node.Id;

            if (node.Areas != null)
                foreach (var child in node.Areas)
                    Traverse(child);
        }
    }
}