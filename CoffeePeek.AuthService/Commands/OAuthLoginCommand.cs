using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.Auth;
using MediatR;

namespace CoffeePeek.AuthService.Commands;

public record GoogleLoginCommand(string IdToken) : IRequest<Response<GoogleLoginResponse>>;