using System.Text.Json.Serialization;

namespace CoffeePeek.Account.Application.Features.Auth.RefreshToken;

public record RefreshTokenCommand(
    [property: JsonIgnore] Guid UserId,
    string RefreshToken,
    [property: JsonIgnore] string DeviceName = "unknown",
    [property: JsonIgnore] string IpAddress = "unknown");