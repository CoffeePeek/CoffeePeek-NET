namespace CoffeePeek.Shops.Domain.Abstracts;

public interface IEntity
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
    void RemoveDomainEvent(IDomainEvent eventItem);
}