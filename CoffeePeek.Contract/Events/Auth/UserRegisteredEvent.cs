namespace CoffeePeek.Contract.Events;

public record UserRegisteredEvent(Guid UserId, string Email, string UserName);