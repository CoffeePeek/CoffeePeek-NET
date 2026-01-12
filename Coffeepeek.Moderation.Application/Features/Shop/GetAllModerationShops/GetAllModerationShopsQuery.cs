using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.CoffeeShop;
using MediatR;

namespace Coffeepeek.Moderation.Application.Features.Shop.GetAllModerationShops;

public class GetAllModerationShopsQuery : IRequest<Response<GetCoffeeShopsInModerationByIdResponse>>;

