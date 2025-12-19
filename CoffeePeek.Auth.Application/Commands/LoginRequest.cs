using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.Login;
using MediatR;

namespace CoffeePeek.Auth.Application.Commands;

public class LoginRequest(string email, string password) : IRequest<Response<LoginResponse>>
{
    public string Email { get; } = email;
    public string Password { get; } = password;
}