using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.CoffeeShop;
using CoffeePeek.Data.Interfaces;
using CoffeePeek.ShopsService.Entities;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.ShopsService.Handlers.CoffeeShop;

public class GetCoffeeShopHandler(IGenericRepository<Shop> shopRepository, IMapper mapper)
    : IRequestHandler<GetCoffeeShopCommand, Response<GetCoffeeShopResponse>>
{
    public async Task<Response<GetCoffeeShopResponse>> Handle(GetCoffeeShopCommand request,
        CancellationToken cancellationToken)
    {
        var shopDto = await shopRepository
            .QueryAsNoTracking()
            .ProjectToType<ShopDto>(mapper.Config)
            .SingleOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        return shopDto == null
            ? Response<GetCoffeeShopResponse>.Error($"Coffee shop with ID {request.Id} not found.")
            : Response<GetCoffeeShopResponse>.Success(new GetCoffeeShopResponse(shopDto));
    }
}