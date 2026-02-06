namespace CoffeePeek.Shared.Infrastructure.Abstract;

public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot
{
}

public interface IAggregateRoot : IEntity { }   