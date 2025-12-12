using CoffeePeek.Contract.Response.Auth;
using CoffeePeek.Contract.Responses;
using MediatR;

namespace CoffeePeek.AuthService.Commands;

public record GoogleLoginCommand(string IdToken) : IRequest<Response<GoogleLoginResponse>>;