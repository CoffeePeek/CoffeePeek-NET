using CoffeePeek.Contract.Requests.Internal;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.Internal;
using CoffeePeek.ShopsService.Services.Interfaces;
using MediatR;

namespace CoffeePeek.ShopsService.Handlers.Internal;

public class GetAllRoastersHandler(ICacheService cacheService) : IRequestHandler<GetAllRoastersCommand, Response<GetAllRoastersResponse>>
{
    public async Task<Response<GetAllRoastersResponse>> Handle(GetAllRoastersCommand request, CancellationToken cancellationToken)
    {
        var roasters = await cacheService.GetRoasters();
        
        var response = new GetAllRoastersResponse(roasters);
        
        return Response<GetAllRoastersResponse>.Success(response);
    }
}