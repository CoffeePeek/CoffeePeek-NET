using CoffeePeek.Shared.Domain.Events;

namespace CoffeePeek.Account.Domain.Events;

public record EmailConfirmedInternalEvent : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}