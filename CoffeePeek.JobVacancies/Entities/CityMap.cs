namespace CoffeePeek.JobVacancies.Entities;

public class CityMap
{
    public Guid Id { get; set; }
    public Guid CityId { get; set; }
    public string CityName { get; set; }
    public int HhAreaId { get; set; }
    public DateTime UpdatedAt { get; set; }
}

