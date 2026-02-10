using System.Text.Json.Serialization;

namespace CoffeePeek.Account.Application.Features.User.UpdateUserProfile.UpdateUsername;

public record UpdateProfileUsernameCommand([property: JsonIgnore] Guid UserId, string Username);