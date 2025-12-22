using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.Auth;
using MediatR;

namespace CoffeePeek.Account.Application.Commands;

public class RefreshTokenCommand(
    string refreshToken,
    string deviceName = "unknown",
    string ipAddress = "unknown") : IRequest<Response<GetRefreshTokenResponse>>
{
    [Required]
    public string RefreshToken { get; } = refreshToken;
    [JsonIgnore]
    public Guid UserId { get; set; }

    [JsonIgnore]
    public string DeviceName { get; } = deviceName;

    [JsonIgnore]
    public string IpAddress { get; } = ipAddress;
}