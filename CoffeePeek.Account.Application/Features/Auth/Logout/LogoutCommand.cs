namespace CoffeePeek.Account.Application.Features.Auth.Logout;

public record LogoutCommand(Guid UserId, string RefreshToken);
