namespace CoffeePeek.Shared.Infrastructure.Abstract;

public interface IAuditableEntity
{
    public DateTime CreatedAtUtc { get; set; } 
    public DateTime? UpdatedAtUtc { get; set; }
}