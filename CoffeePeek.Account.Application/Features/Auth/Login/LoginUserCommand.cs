using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.Login;
using MediatR;

namespace CoffeePeek.Account.Application.Features.Auth.Login;

public record LoginUserCommand(
    string Email,
    string Password,
    string DeviceName = "unknown",
    string IpAddress = "unknown")
    : IRequest<Response<LoginResponse>>;