namespace CoffeePeek.Shared.Infrastructure.Abstract;

public interface IEntity
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}