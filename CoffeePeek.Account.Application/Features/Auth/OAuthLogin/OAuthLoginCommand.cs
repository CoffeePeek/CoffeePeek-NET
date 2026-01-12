using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.Auth;
using MediatR;

namespace CoffeePeek.Account.Application.Features.Auth.OAuthLogin;

public record GoogleLoginCommand(
    string IdToken,
    string DeviceName = "unknown",
    string IpAddress = "unknown") : IRequest<Response<GoogleLoginResponse>>;