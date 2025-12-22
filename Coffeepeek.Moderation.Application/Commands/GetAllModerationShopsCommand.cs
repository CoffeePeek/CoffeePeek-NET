using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.CoffeeShop;
using MediatR;

namespace CoffeePeek.Moderation.Application.Commands;

public class GetAllModerationShopsCommand : IRequest<Response<GetCoffeeShopsInModerationByIdResponse>>;

