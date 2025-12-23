using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.CoffeeShop;
using MediatR;

namespace Coffeepeek.Moderation.Application.Features.GetAllModerationShops;

public class GetAllModerationShopsCommand : IRequest<Response<GetCoffeeShopsInModerationByIdResponse>>;

