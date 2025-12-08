using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.Login;
using MediatR;

namespace CoffeePeek.AuthService.Commands;

public record LoginUserCommand(string Email, string Password) : IRequest<Response<LoginResponse>>;