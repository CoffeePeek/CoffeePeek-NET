using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Responses;
using MediatR;

namespace CoffeePeek.Account.Application.Features.Auth.OAuthLogin;

public record GoogleLoginCommand(
    string IdToken,
    string DeviceName = "unknown",
    string IpAddress = "unknown") : IRequest<Response<GoogleLoginResponse>>;