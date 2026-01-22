using System.Text.Json.Serialization;
using CoffeePeek.Contract.Abstract;
using MediatR;

namespace CoffeePeek.Account.Application.Features.Auth.RefreshToken;

public record RefreshTokenCommand(
    [property: JsonIgnore] Guid UserId,
    string RefreshToken,
    string DeviceName = "unknown",
    string IpAddress = "unknown") : IRequest<Response<RefreshTokenResponse>>;