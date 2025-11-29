using CoffeePeek.Contract.Requests.Internal;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.Internal;
using CoffeePeek.Infrastructure.Cache.Interfaces;
using MediatR;

namespace CoffeePeek.BusinessLogic.RequestHandlers.Internal;

public class GetCitiesRequestHandler(ICacheService cacheService) : IRequestHandler<GetCitiesRequest, Response<GetCitiesResponse>>
{
    public async Task<Response<GetCitiesResponse>> Handle(GetCitiesRequest request, CancellationToken cancellationToken)
    {
        var cities = await cacheService.GetCities();
        
        var response = new GetCitiesResponse(cities.ToArray());
        
        return Response.SuccessResponse<Response<GetCitiesResponse>>(response);
    }
}