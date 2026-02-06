using CoffeePeek.Contract.Abstract;
using CoffeePeek.Shops.Application.Services;
using MediatR;

namespace CoffeePeek.Shops.Application.Features.Catalogs.GetAllBrewMethods;

public class GetAllBrewMethodsHandler(ICacheService cacheService) : IRequestHandler<GetAllBrewMethodsCommand, Response<GetAllBrewMethodsResponse>>
{
    public async Task<Response<GetAllBrewMethodsResponse>> Handle(GetAllBrewMethodsCommand request, CancellationToken cancellationToken)
    {
        var brewMethods = await cacheService.GetBrewMethods();
        
        var response = new GetAllBrewMethodsResponse(brewMethods);
        
        return Response<GetAllBrewMethodsResponse>.Success(response);
    }
}