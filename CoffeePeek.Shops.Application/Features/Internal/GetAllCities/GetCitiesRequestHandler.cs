using CoffeePeek.Contract.Abstract;
using CoffeePeek.Shops.Application.Services;
using MediatR;

namespace CoffeePeek.Shops.Application.Features.Internal.GetAllCities;

public class GetCitiesRequestHandler(ICacheService cacheService) : IRequestHandler<GetCitiesCommand, Response<GetCitiesResponse>>
{
    public async Task<Response<GetCitiesResponse>> Handle(GetCitiesCommand command, CancellationToken cancellationToken)
    {
        var cities = await cacheService.GetCities();

        if (cities == null)
        {
            throw new ApplicationException("No cities found");
        }
        
        var response = new GetCitiesResponse(cities.ToArray());
        
        return Response<GetCitiesResponse>.Success(response);
    }
}