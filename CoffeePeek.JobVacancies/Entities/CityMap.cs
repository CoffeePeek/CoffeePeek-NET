using System.ComponentModel.DataAnnotations;

namespace CoffeePeek.JobVacancies.Entities;

public class CityMap
{
    public Guid Id { get; set; }
    public Guid CityId { get; set; }
    public required string CityName { get; set; } = null!;
    public int HhAreaId { get; set; }
    public DateTime UpdatedAt { get; set; }
}

