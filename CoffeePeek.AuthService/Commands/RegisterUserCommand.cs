using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.Auth;
using MediatR;

namespace CoffeePeek.AuthService.Commands;

public class RegisterUserCommand(string userName, string email, string password)
    : IRequest<Response<RegisterUserResponse>>
{
    public string UserName { get; } = userName;
    public string Email { get; } = email;
    public string Password { get; } = password;
}