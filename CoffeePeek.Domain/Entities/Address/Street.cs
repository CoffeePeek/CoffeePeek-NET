using System.ComponentModel.DataAnnotations;

namespace CoffeePeek.Domain.Entities.Address;

public class Street : BaseEntity
{
    [MaxLength(70)]
    public string Name { get; set; }
    public int CityId { get; set; }

    public City City { get; set; }
}