using System.ComponentModel.DataAnnotations;

namespace CoffeePeek.Domain.Entities.Address;

public class Country : BaseEntity
{
    [MaxLength(45)]
    public string Name { get; set; }
}