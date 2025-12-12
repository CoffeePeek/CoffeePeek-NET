namespace CoffeePeek.JobVacancies.Models.Dtos;

public record JobVacancyDto(
    string ExternalId,
    string Source,
    string Title,
    string Company,
    string Url,
    string? ProfessionalRole,
    string? Area,
    int? SalaryFrom,
    int? SalaryTo,
    string? Currency,
    DateTime PublishedAt);