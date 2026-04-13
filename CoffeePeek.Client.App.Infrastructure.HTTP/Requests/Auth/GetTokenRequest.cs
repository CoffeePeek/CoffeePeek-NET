namespace CoffeePeek.Client.App.Infrastructure.HTTP.Requests.Auth;

public sealed class GetTokenRequest : BaseRequest
{
    public required string Email { get; init; }

    public required string Password { get; init; }
}
