namespace CoffeePeek.Shared.Domain.Entities;

public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot
{
}

public interface IAggregateRoot : IEntity { }   