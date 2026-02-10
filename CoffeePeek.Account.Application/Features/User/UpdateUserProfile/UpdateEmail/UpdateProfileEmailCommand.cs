using System.Text.Json.Serialization;

namespace CoffeePeek.Account.Application.Features.User.UpdateUserProfile.UpdateEmail;

public record UpdateProfileEmailCommand([property: JsonIgnore] Guid UserId, string Email);