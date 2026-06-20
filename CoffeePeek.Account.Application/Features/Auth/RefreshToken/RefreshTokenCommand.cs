namespace CoffeePeek.Account.Application.Features.Auth.RefreshToken;

public record RefreshTokenCommand(
    string RefreshToken,
    string DeviceName = "unknown",
    string IpAddress = "unknown");