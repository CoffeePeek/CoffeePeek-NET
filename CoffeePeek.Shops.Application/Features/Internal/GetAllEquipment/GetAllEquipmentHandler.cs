using CoffeePeek.Contract.Abstract;
using CoffeePeek.Shops.Application.Services;
using MediatR;

namespace CoffeePeek.Shops.Application.Features.Internal.GetAllEquipment;

public class GetAllEquipmentHandler(ICacheService cacheService) : IRequestHandler<GetAllEquipmentCommand, Response<GetAllEquipmentResponse>>
{
    public async Task<Response<GetAllEquipmentResponse>> Handle(GetAllEquipmentCommand request, CancellationToken cancellationToken)
    {
        var equipments = await cacheService.GetEquipments();
        
        var response = new GetAllEquipmentResponse(equipments);
        
        return Response<GetAllEquipmentResponse>.Success(response);
    }
}

