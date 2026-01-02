using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.Internal;
using CoffeePeek.Shops.Application.Commands.Internal;
using CoffeePeek.Shops.Application.Services;
using MediatR;

namespace CoffeePeek.Shops.Application.Handlers.Internal;

public class GetAllBeansHandler(ICacheService cacheService) : IRequestHandler<GetAllBeansCommand, Response<GetAllBeansResponse>>
{
    public async Task<Response<GetAllBeansResponse>> Handle(GetAllBeansCommand request, CancellationToken cancellationToken)
    {
        var beans = await cacheService.GetBeans();
        
        var response = new GetAllBeansResponse(beans);
        
        return Response<GetAllBeansResponse>.Success(response);
    }
}