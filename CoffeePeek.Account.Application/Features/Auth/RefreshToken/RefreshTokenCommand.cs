namespace CoffeePeek.Account.Application.Features.Auth.RefreshToken;

public record RefreshTokenCommand(
    Guid UserId,
    string RefreshToken,
    string DeviceName = "unknown",
    string IpAddress = "unknown");