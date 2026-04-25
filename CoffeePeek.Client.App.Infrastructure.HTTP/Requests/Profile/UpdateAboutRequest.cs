namespace CoffeePeek.Client.App.Infrastructure.HTTP.Requests.Profile;

public sealed class UpdateAboutRequest : BaseRequest
{
    public required string About { get; init; }
}
