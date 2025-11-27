using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CoffeePeek.Contract.Response.Auth;
using MediatR;

namespace CoffeePeek.Contract.Requests.Auth;

public class GetRefreshTokenRequest(string refreshToken) : IRequest<Response.Response<GetRefreshTokenResponse>>
{
    [Required]
    public string RefreshToken { get; } = refreshToken;
    [JsonIgnore]
    public int UserId { get; set; }
}