using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.Internal;
using MediatR;

namespace CoffeePeek.Shops.Application.Commands.Internal;

public record GetAllBeansCommand : IRequest<Response<GetAllBeansResponse>>;