using CoffeePeek.Contract.Requests.Internal;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.Internal;
using CoffeePeek.ShopsService.Services.Interfaces;
using MediatR;

namespace CoffeePeek.ShopsService.Handlers.Internal;

public class GetCitiesRequestHandler(ICacheService cacheService) : IRequestHandler<GetCitiesCommand, Response<GetCitiesResponse>>
{
    public async Task<Response<GetCitiesResponse>> Handle(GetCitiesCommand command, CancellationToken cancellationToken)
    {
        var cities = await cacheService.GetCities();
        
        var response = new GetCitiesResponse(cities.ToArray());
        
        return Response<GetCitiesResponse>.Success(response);
    }
}