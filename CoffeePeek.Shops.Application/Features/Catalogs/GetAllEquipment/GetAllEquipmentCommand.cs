using CoffeePeek.Contract.Abstract;
using MediatR;

namespace CoffeePeek.Shops.Application.Features.Catalogs.GetAllEquipment;

public record GetAllEquipmentCommand : IRequest<Response<GetAllEquipmentResponse>>;