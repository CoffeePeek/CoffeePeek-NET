namespace CoffeePeek.Account.Application.Features.Auth.OAuthLogin;

public class GoogleLoginUser
{
    public required string Email { get; init; }
    public required string Username { get; init; }
    public required string AvatarUrl { get; init; }
}