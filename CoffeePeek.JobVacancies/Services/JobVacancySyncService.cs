using CoffeePeek.JobVacancies.Entities;
using CoffeePeek.JobVacancies.Models;
using CoffeePeek.JobVacancies.Repository;
using CoffeePeek.JobVacancies.Utils;
using CoffeePeek.Shared.Infrastructure.Cache;
using CoffeePeek.Shared.Infrastructure.Interfaces.Redis;
using MapsterMapper;

namespace CoffeePeek.JobVacancies.Services;

public class JobVacancySyncService(
    IMapper mapper,
    IJobVacancyRepository repository,
    ICityRepository cityRepository,
    IRedisService redisService,
    ILogger<JobVacancySyncService> logger) : IJobVacancySyncService
{
    private readonly AsyncLock _asyncLock = new();

    public async Task SyncVacanciesAsync(List<HhVacancyItem> vacancies, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var mapped = mapper.Map<List<JobVacancy>>(vacancies);

            var mappedDistinct = mapped
                .GroupBy(x => x.ExternalId)
                .Select(g => g.First())
                .ToList();

            var hhAreaIds = vacancies
                .Where(v => v.Area?.Id != null && int.TryParse(v.Area.Id, out _))
                .Select(v => int.Parse(v.Area!.Id!))
                .Distinct()
                .ToList();

            var cityMaps = await cityRepository.GetByHhAreaIdsAsync(hhAreaIds, cancellationToken);

            var vacanciesByExternalId = vacancies
                .GroupBy(x => x.Id)
                .ToDictionary(v => v.Key, v => v.First());

            foreach (var vacancy in mappedDistinct)
            {
                if (!vacanciesByExternalId.TryGetValue(vacancy.ExternalId, out var hhVacancy)) continue;
                if (hhVacancy.Area?.Id == null || !int.TryParse(hhVacancy.Area.Id, out var hhAreaId)) continue;
                if (cityMaps.TryGetValue(hhAreaId, out var cityMap))
                {
                    vacancy.CityId = cityMap.CityId;
                    vacancy.CityMapId = cityMap.Id;
                }
                else
                {
                    logger.LogWarning("CityMap not found for hhAreaId: {HhAreaId}, vacancy: {VacancyId}",
                        hhAreaId, vacancy.ExternalId);
                }
            }

            var externalIds = mappedDistinct.Select(x => x.ExternalId).ToList();
            var existing = await repository.GetByExternalIdsAsync(externalIds, cancellationToken);
            var existingDict = existing.ToDictionary(x => x.ExternalId, x => x);

            var jobsToUpsert = new List<JobVacancy>();
            
            foreach (var newJob in mappedDistinct)
            {
                if (existingDict.TryGetValue(newJob.ExternalId, out var existingJob))
                {
                    existingJob.Title = newJob.Title;
                    existingJob.Company = newJob.Company;
                    existingJob.CityId = newJob.CityId;
                    existingJob.CityMapId = newJob.CityMapId;
                    existingJob.SyncedAt = DateTime.UtcNow;
                    
                    jobsToUpsert.Add(existingJob); 
                }
                else
                {
                    newJob.SyncedAt = DateTime.UtcNow;
                    jobsToUpsert.Add(newJob);
                }
            }

            if (jobsToUpsert.Count != 0)
                await repository.UpsertRangeAsync(jobsToUpsert, cancellationToken);

            await redisService.RemoveByPatternAsync(CacheKey.Vacancies.AllPattern());
        }
    }
}