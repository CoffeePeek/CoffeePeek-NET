using CoffeePeek.Moderation.Application.Requests;
using CoffeePeek.Moderation.Application.Responses;
using CoffeePeek.Moderation.Contract.Abstract;
using MediatR;

namespace CoffeePeek.Moderation.Application.Handlers.Auth;

public class GetRefreshTokenRequestHandler : IRequestHandler<GetRefreshTokenRequest, Response<GetRefreshTokenResponse>>
{
    public Task<Response<GetRefreshTokenResponse>> Handle(GetRefreshTokenRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}