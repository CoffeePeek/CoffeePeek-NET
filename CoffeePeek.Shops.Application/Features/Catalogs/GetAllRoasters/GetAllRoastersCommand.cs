using CoffeePeek.Contract.Abstract;
using MediatR;

namespace CoffeePeek.Shops.Application.Features.Catalogs.GetAllRoasters;

public record GetAllRoastersCommand : IRequest<Response<GetAllRoastersResponse>>;