namespace CoffeePeek.Shared.Infrastructure.Abstract;

public abstract class Entity<TId> : IEntity, IAuditableEntity
{
    private readonly List<IDomainEvent> _domainEvents = []; 

    public TId Id { get; protected init; } = default!;
    public DateTime CreatedAtUtc { get; set; } 
    public DateTime? UpdatedAtUtc { get; set; }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(IDomainEvent eventItem)
    {
        _domainEvents.Add(eventItem);
    }

    public void RemoveDomainEvent(IDomainEvent eventItem)
    {
        _domainEvents.Remove(eventItem);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}