using CoffeePeek.JobVacancies.Configuration;
using CoffeePeek.JobVacancies.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.JobVacancies.Repository;

public class JobVacancyRepository(JobVacanciesDbContext dbContext) : IJobVacancyRepository
{
    private readonly DbSet<JobVacancy> _vacancies = dbContext.JobVacancies;

    public async Task<JobVacancy[]> GetAllByCityIdWithPagination(Guid cityId, CPJobType jobType, int page, int perPage,
        CancellationToken cancellationToken)
    {
        return await _vacancies
            .AsNoTracking()
            .Where(x => x.CityId == cityId)
            .Where(x => x.Type == jobType)
            .OrderByDescending(x => x.PublishedAt)
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .ToArrayAsync(cancellationToken);
    }

    public async Task UpsertRangeAsync(IReadOnlyList<JobVacancy> items, CancellationToken cancellationToken = default)
    {
        await _vacancies.BulkMergeAsync(items, cancellationToken);
    }

    public async Task<IReadOnlyList<JobVacancy>> GetByExternalIdsAsync(IReadOnlyList<string>? externalIds,
        CancellationToken cancellationToken)
    {
        if (externalIds == null || externalIds.Count == 0)
            return [];

        return await _vacancies
            .Where(x => externalIds.Contains(x.ExternalId))
            .ToListAsync(cancellationToken);
    }
}