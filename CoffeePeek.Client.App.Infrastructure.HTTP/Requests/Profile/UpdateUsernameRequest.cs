namespace CoffeePeek.Client.App.Infrastructure.HTTP.Requests.Profile;

public sealed class UpdateUsernameRequest : BaseRequest
{
    public required string Username { get; init; }
}
