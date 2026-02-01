using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Responses;
using MediatR;
using Newtonsoft.Json;

namespace CoffeePeek.Account.Application.Features.Auth.OAuthLogin;

public record GoogleLoginCommand(
    string IdToken,
    [property:JsonIgnore] string DeviceName = "unknown",
    [property:JsonIgnore] string IpAddress = "unknown") : IRequest<Response<GoogleLoginResponse>>;