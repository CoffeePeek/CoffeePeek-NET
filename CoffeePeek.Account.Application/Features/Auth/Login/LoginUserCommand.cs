using System.Text.Json.Serialization;

namespace CoffeePeek.Account.Application.Features.Auth.Login;

public record LoginUserCommand(
    string Email,
    string Password,
    [property:JsonIgnore] string DeviceName = "unknown",
    [property:JsonIgnore] string IpAddress = "unknown");