using CoffeePeek.Shared.Domain.Events;

namespace CoffeePeek.Account.Domain.Entities.UserAggregate;

public record PasswordResetRequestedInternalEvent(
    Guid UserId,
    string Email,
    string Username,
    string ResetToken) : IImmediateEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
