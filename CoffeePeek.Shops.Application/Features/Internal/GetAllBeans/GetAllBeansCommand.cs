using CoffeePeek.Contract.Abstract;
using MediatR;

namespace CoffeePeek.Shops.Application.Features.Internal.GetAllBeans;

public record GetAllBeansCommand : IRequest<Response<GetAllBeansResponse>>;