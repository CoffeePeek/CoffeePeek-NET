using System.Text.Json.Serialization;

namespace CoffeePeek.Account.Application.Features.Auth.OAuthLogin;

public record GoogleLoginCommand(
    string IdToken,
    [property:JsonIgnore] string DeviceName = "unknown",
    [property:JsonIgnore] string IpAddress = "unknown");