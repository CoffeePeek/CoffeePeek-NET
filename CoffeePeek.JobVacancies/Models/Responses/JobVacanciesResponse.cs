using CoffeePeek.JobVacancies.Models.Dtos;

namespace CoffeePeek.JobVacancies.Models.Responses;

public class JobVacanciesResponse
{
    public int Page { get; init; }
    public int PerPage { get; init; }
    public int Found { get; init; }
    public int Total { get; init; }
    public int TotalPages { get; init; }
    public IReadOnlyCollection<JobVacancyDto> Items { get; init; }
}


