using System.ComponentModel.DataAnnotations.Schema;

namespace CoffeePeek.JobVacancies.Entities;

public class JobVacancy
{
    public Guid Id { get; set; }

    // External source identifiers/metadata
    public string ExternalId { get; set; } = null!;
    public string Source { get; set; } = "hh.ru";

    // Core vacancy info
    public string Title { get; set; } = null!;
    public string Company { get; set; } = null!;
    public string Url { get; set; } = null!;
    public CPJobType Type { get; set; }
    public string? ProfessionalRole { get; set; }
    public string? Area { get; set; }
    public Guid CityMapId { get; set; }
    public Guid CityId{ get; set; }

    // Compensation
    public int? SalaryFrom { get; set; }
    public int? SalaryTo { get; set; }
    public string? Currency { get; set; }

    // Timestamps
    public DateTime PublishedAt { get; set; }
    public DateTime SyncedAt { get; set; }
    
    public virtual CityMap CityMap { get; set; }
}