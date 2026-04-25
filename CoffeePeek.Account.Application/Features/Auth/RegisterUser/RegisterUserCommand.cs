namespace CoffeePeek.Account.Application.Features.Auth.RegisterUser;

public record RegisterUserCommand(string UserName, string Email, string Password);