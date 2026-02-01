namespace CoffeePeek.Account.Application.Features.Auth.Login;

public class LoginRequest(string email, string password)
{
    public string Email { get; } = email;
    public string Password { get; } = password;
}