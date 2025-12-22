using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.Auth;
using MediatR;

namespace CoffeePeek.Account.Application.Commands;

public class GetRefreshTokenCommand(string refreshToken) : IRequest<Response<GetRefreshTokenResponse>>
{
    [Required]
    public string RefreshToken { get; } = refreshToken;
    [JsonIgnore]
    public int UserId { get; set; }
}