using CoffeePeek.Contract.Abstract;
using MediatR;

namespace CoffeePeek.Shops.Application.Features.Internal.GetAllRoasters;

public record GetAllRoastersCommand : IRequest<Response<GetAllRoastersResponse>>;