using CoffeePeek.JobVacancies.Domain.Entities;
using CoffeePeek.JobVacancies.Domain.Repositories;
using CoffeePeek.Shared.Infrastructure.Abstract;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.JobVacancies.Application.Repositories;

public class JobVacancyRepository(IGenericRepository<JobVacancy> jobVacanciesRepository) : IJobVacancyRepository
{
    public async Task<JobVacancy[]> GetAllByCityIdWithPagination(Guid cityId, CPJobType jobType, int page, int perPage,
        CancellationToken cancellationToken)
    {
        return await jobVacanciesRepository
            .QueryAsNoTracking()
            .Where(x => x.CityId == cityId)
            .Where(x => x.Type == jobType)
            .OrderByDescending(x => x.PublishedAt)
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .ToArrayAsync(cancellationToken);
    }

    public async Task UpsertRangeAsync(IReadOnlyList<JobVacancy> items, CancellationToken cancellationToken = default)
    {
        await jobVacanciesRepository.BulkMergeAsync(items, cancellationToken);
    }

    public async Task<IReadOnlyList<JobVacancy>> GetByExternalIdsAsync(IReadOnlyList<string>? externalIds,
        CancellationToken cancellationToken)
    {
        if (externalIds == null || externalIds.Count == 0)
            return [];

        return await jobVacanciesRepository.QueryAsNoTracking()
            .Where(x => externalIds.Contains(x.ExternalId))
            .ToListAsync(cancellationToken);
    }
}