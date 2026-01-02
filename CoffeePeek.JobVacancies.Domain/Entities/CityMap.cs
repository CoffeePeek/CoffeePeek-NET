using System.ComponentModel.DataAnnotations;

namespace CoffeePeek.JobVacancies.Domain.Entities;

public class CityMap
{
    public Guid Id { get; set; }
    public Guid CityId { get; set; }
    [MaxLength(30)]
    public required string CityName { get; set; } = null!;
    public int HhAreaId { get; set; }
    public DateTime UpdatedAt { get; set; }
}

