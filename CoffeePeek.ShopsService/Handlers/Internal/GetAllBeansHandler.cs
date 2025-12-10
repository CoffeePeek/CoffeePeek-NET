using CoffeePeek.Contract.Requests.Internal;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.Internal;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.Internal;
using CoffeePeek.ShopsService.Services.Interfaces;
using MediatR;

namespace CoffeePeek.ShopsService.Handlers.Internal;

public class GetAllBeansHandler(ICacheService cacheService) : IRequestHandler<GetAllBeansCommand, Response<GetAllBeansResponse>>
{
    public async Task<Response<GetAllBeansResponse>> Handle(GetAllBeansCommand request, CancellationToken cancellationToken)
    {
        var beans = await cacheService.GetBeans();
        
        var response = new GetAllBeansResponse(beans);
        
        return Response<GetAllBeansResponse>.Success(response);
    }
}