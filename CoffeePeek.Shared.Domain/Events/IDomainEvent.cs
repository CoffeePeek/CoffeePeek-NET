
namespace CoffeePeek.Shared.Domain.Events;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}