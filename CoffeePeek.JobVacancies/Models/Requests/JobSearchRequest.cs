using CoffeePeek.JobVacancies.Entities;

namespace CoffeePeek.JobVacancies.Models.Requests;

public record JobSearchRequest(
    string Text,
    int Page,
    int PerPage,
    string? Area,
    string? Role,
    CPJobType? JobType,
    Guid? CityId);

