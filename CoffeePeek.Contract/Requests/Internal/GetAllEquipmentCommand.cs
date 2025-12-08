using CoffeePeek.Contract.Response.Internal;
using MediatR;

namespace CoffeePeek.Contract.Requests.Internal;

public record GetAllEquipmentCommand : IRequest<Response.Response<GetAllEquipmentResponse>>;