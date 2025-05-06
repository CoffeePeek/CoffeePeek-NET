using CoffeePeek.Moderation.Application.Responses;
using CoffeePeek.Moderation.Contract.Abstract;
using MediatR;

namespace CoffeePeek.Moderation.Application.Requests;

public class GetRefreshTokenRequest(string refreshToken) : IRequest<Response<GetRefreshTokenResponse>>
{
    public string RefreshToken { get; set; } = refreshToken;
}