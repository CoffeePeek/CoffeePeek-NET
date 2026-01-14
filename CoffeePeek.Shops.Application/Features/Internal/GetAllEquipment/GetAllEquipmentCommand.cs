using CoffeePeek.Contract.Abstract;
using MediatR;

namespace CoffeePeek.Shops.Application.Features.Internal.GetAllEquipment;

public record GetAllEquipmentCommand : IRequest<Response<GetAllEquipmentResponse>>;