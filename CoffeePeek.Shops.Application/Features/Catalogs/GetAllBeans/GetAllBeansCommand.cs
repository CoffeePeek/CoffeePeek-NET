using CoffeePeek.Contract.Abstract;
using MediatR;

namespace CoffeePeek.Shops.Application.Features.Catalogs.GetAllBeans;

public record GetAllBeansCommand : IRequest<Response<GetAllBeansResponse>>;