using CoffeePeek.Shared.Domain.Events;

namespace CoffeePeek.Shared.Domain.Entities;

public interface IEntity
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
    void RemoveDomainEvent(IDomainEvent eventItem);
}