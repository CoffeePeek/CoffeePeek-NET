using CoffeePeek.Contract.Abstract;
using CoffeePeek.Shops.Application.Services;
using MediatR;

namespace CoffeePeek.Shops.Application.Features.Internal.GetAllRoasters;

public class GetAllRoastersHandler(ICacheService cacheService) : IRequestHandler<GetAllRoastersCommand, Response<GetAllRoastersResponse>>
{
    public async Task<Response<GetAllRoastersResponse>> Handle(GetAllRoastersCommand request, CancellationToken cancellationToken)
    {
        var roasters = await cacheService.GetRoasters();
        
        var response = new GetAllRoastersResponse(roasters);
        
        return Response<GetAllRoastersResponse>.Success(response);
    }
}