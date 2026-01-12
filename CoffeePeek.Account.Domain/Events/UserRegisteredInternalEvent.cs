using CoffeePeek.Shared.Infrastructure.Abstract;

namespace CoffeePeek.Account.Domain.Events;

public record UserRegisteredInternalEvent(
    Guid UserId, 
    string Email, 
    string Username, 
    string ConfirmationToken) : IImmediateEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}