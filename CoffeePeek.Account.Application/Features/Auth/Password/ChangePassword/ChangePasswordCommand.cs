using System.Text.Json.Serialization;

namespace CoffeePeek.Account.Application.Features.Auth.Password.ChangePassword;

public record ChangePasswordCommand(
    [property: JsonIgnore] Guid UserId,
    string CurrentPassword,
    string NewPassword);
