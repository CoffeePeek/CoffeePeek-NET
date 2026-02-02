using CoffeePeek.Contract.Abstract;
using CoffeePeek.Shops.Application.Services;
using MediatR;

namespace CoffeePeek.Shops.Application.Features.Catalogs.GetAllBeans;

public class GetAllBeansHandler(ICacheService cacheService) : IRequestHandler<GetAllBeansCommand, Response<GetAllBeansResponse>>
{
    public async Task<Response<GetAllBeansResponse>> Handle(GetAllBeansCommand request, CancellationToken cancellationToken)
    {
        var beans = await cacheService.GetBeans();
        
        var response = new GetAllBeansResponse(beans);
        
        return Response<GetAllBeansResponse>.Success(response);
    }
}