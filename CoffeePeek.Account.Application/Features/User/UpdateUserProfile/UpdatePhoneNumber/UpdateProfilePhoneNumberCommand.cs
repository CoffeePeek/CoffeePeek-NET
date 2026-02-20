using System.Text.Json.Serialization;

namespace CoffeePeek.Account.Application.Features.User.UpdateUserProfile.UpdatePhoneNumber;

public record UpdateProfilePhoneNumberCommand(
    [property: JsonIgnore] Guid UserId,
    string PhoneNumber);
