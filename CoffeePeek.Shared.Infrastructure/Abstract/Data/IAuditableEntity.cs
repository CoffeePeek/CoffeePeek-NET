namespace CoffeePeek.Shared.Infrastructure.Abstract;

public interface IAuditableEntity
{
    public DateTime CreatedAtUtc { get; init; } 
    public DateTime? UpdatedAtUtc { get; protected set; }
}