using System.Text.Json.Serialization;

namespace CoffeePeek.Account.Application.Features.User.UpdateUserProfile.UpdateAbout;

public record UpdateProfileAboutCommand(
    [property: JsonIgnore] Guid UserId,
    string About);
