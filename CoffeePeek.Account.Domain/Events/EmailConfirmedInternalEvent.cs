using CoffeePeek.Shared.Infrastructure.Abstract;

namespace CoffeePeek.Account.Domain.Events;

public record EmailConfirmedInternalEvent : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}