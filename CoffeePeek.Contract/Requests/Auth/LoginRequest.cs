using CoffeePeek.Contract.Response.Login;
using CoffeePeek.Contract.Responses;
using MediatR;

namespace CoffeePeek.Contract.Requests.Auth;

public class LoginRequest(string email, string password) : IRequest<Response<LoginResponse>>
{
    public string Email { get; } = email;
    public string Password { get; } = password;
}