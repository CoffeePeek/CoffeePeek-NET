using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.CoffeeShop;
using MediatR;

namespace Coffeepeek.Moderation.Application.Commands;

public class GetAllModerationShopsCommand : IRequest<Response<GetCoffeeShopsInModerationByIdResponse>>;

