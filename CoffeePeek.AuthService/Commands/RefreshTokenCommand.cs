using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.Auth;
using MediatR;

namespace CoffeePeek.AuthService.Commands;

public class RefreshTokenCommand(string refreshToken) : IRequest<Response<GetRefreshTokenResponse>>
{
    [Required]
    public string RefreshToken { get; } = refreshToken;
    [JsonIgnore]
    public Guid UserId { get; set; }
}