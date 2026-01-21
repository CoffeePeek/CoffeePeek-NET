namespace CoffeePeek.Contract.Events.Account;

public record UserNameChangedEvent
{
    public Guid UserId { get; init; }
    public string NewUserName { get; init; } = string.Empty;
    public DateTime ChangedAt { get; init; }
}