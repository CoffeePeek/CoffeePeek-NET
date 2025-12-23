using CoffeePeek.Shared.Infrastructure.Abstract;

public abstract class Entity<TId> : IEntity
{
    private readonly List<IDomainEvent> _domainEvents = []; 

    public virtual TId Id { get; protected init; } = default!;

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