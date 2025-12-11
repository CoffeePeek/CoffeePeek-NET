using CoffeePeek.JobVacancies.Entities;

namespace CoffeePeek.JobVacancies.Repository;

public interface IJobVacancyRepository
{
    Task<JobVacancy[]> GetAllByCityIdWithPagination(Guid cityId, CPJobType jobType, int page, int perPage, CancellationToken cancellationToken);
    Task UpsertRangeAsync(IReadOnlyList<JobVacancy> toList, CancellationToken cancellationToken);
    Task<IReadOnlyList<JobVacancy>> GetByExternalIdsAsync(IReadOnlyList<string> externalIds, CancellationToken cancellationToken);
}