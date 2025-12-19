using CoffeePeek.Contract.Requests.Internal;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.Internal;
using CoffeePeek.Shops.Application.Services;
using MediatR;

namespace CoffeePeek.ShopsService.Handlers.Internal;

public class GetAllBrewMethodsHandler(ICacheService cacheService) : IRequestHandler<GetAllBrewMethodsCommand, Response<GetAllBrewMethodsResponse>>
{
    public async Task<Response<GetAllBrewMethodsResponse>> Handle(GetAllBrewMethodsCommand request, CancellationToken cancellationToken)
    {
        var brewMethods = await cacheService.GetBrewMethods();
        
        var response = new GetAllBrewMethodsResponse(brewMethods);
        
        return Response<GetAllBrewMethodsResponse>.Success(response);
    }
}