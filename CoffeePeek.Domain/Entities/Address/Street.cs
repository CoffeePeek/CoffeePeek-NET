namespace CoffeePeek.Domain.Entities.Address;

public class Street : BaseEntity
{
    public string Name { get; set; }
    public int CityId { get; set; }

    public City City { get; set; }
}