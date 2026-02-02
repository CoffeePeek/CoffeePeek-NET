namespace CoffeePeek.Shops.Domain.Abstracts;

public interface IAuditableEntity
{
    public DateTime CreatedAtUtc { get; set; } 
    public DateTime? UpdatedAtUtc { get; set; }
}