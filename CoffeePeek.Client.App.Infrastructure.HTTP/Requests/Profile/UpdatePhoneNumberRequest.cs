namespace CoffeePeek.Client.App.Infrastructure.HTTP.Requests.Profile;

public sealed class UpdatePhoneNumberRequest : BaseRequest
{
    public required string PhoneNumber { get; init; }
}
