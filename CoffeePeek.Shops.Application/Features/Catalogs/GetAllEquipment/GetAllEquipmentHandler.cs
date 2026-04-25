using CoffeePeek.Contract.Dtos.Shop;
using CoffeePeek.Shared.Domain.Interfaces.Infrastructure;
using CoffeePeek.Shared.Kernel.Response;
using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;
using MapsterMapper;

namespace CoffeePeek.Shops.Application.Features.Catalogs.GetAllEquipment;

public class GetAllEquipmentHandler
{
    public async Task<Response<GetAllEquipmentResponse>> Handle(
        GetAllEquipmentCommand request,
        IQueryEquipmentRepository repository,
        IMapper mapper,
        ICacheService redisService,
        CancellationToken cancellationToken)
    {
        var cacheKey = CacheKey.Equipment.ListAll();
        var result = await redisService.GetAsync<EquipmentDto[]>(cacheKey);

        if (result == null)
        {
            var equipments = await repository.GetAll(cancellationToken);
            result = mapper.Map<EquipmentDto[]>(equipments);
            await redisService.SetAsync(cacheKey, result);
        }
        
        var response = new GetAllEquipmentResponse(result);

        return Response<GetAllEquipmentResponse>.Success(response);
    }
}