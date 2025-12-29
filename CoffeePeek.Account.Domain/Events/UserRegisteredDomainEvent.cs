namespace CoffeePeek.Account.Domain.Events;

public record UserRegisteredDomainEvent(
    Guid UserId, 
    string Email, 
    string Username, 
    string ConfirmationToken) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}