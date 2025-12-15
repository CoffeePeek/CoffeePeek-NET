using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.CoffeeShop;
using MediatR;

namespace CoffeePeek.Contract.Requests.CoffeeShop;

public class GetAllModerationShopsRequest : IRequest<Response<GetCoffeeShopsInModerationByIdResponse>>;

