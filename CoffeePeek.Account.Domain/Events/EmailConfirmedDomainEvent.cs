namespace CoffeePeek.Account.Domain.Events;

public record EmailConfirmedDomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}