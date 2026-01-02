using System.ComponentModel.DataAnnotations;

namespace CoffeePeek.JobVacancies.Domain.Entities;

public class JobVacancy
{
    public Guid Id { get; set; }

    // External source identifiers/metadata
    [MaxLength(55)]
    public string ExternalId { get; set; } = null!;
    [MaxLength(100)]
    public string Source { get; set; } = "hh.ru";

    // Core vacancy info
    [MaxLength(100)]
    public string Title { get; set; } = null!;
    [MaxLength(50)]
    public string Company { get; set; } = null!;
    [MaxLength(255)]
    public string Url { get; set; } = null!;
    public CPJobType Type { get; set; }
    [MaxLength(55)]
    public string? ProfessionalRole { get; set; }
    [MaxLength(50)]
    public string? Area { get; set; }
    public Guid CityMapId { get; set; }
    public Guid CityId{ get; set; }

    // Compensation
    public int? SalaryFrom { get; set; }
    public int? SalaryTo { get; set; }
    [MaxLength(15)]
    public string? Currency { get; set; }

    // Timestamps
    public DateTime PublishedAt { get; set; }
    public DateTime SyncedAt { get; set; }
    
    public virtual CityMap CityMap { get; set; } = null!;
}