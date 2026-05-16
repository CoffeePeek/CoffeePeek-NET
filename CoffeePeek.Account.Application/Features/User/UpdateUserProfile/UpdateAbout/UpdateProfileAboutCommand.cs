using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CoffeePeek.Account.Application.Features.User.UpdateUserProfile.UpdateAbout;

public record UpdateProfileAboutCommand(
    [property: JsonIgnore] Guid UserId,
    [property: MaxLength(600)] string About);
