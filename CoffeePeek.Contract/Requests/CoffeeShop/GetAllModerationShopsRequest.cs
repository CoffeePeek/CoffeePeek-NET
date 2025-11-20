using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.CoffeeShop;
using MediatR;

namespace CoffeePeek.Contract.Requests.CoffeeShop;

public class GetAllModerationShopsRequest : IRequest<Response<GetCoffeeShopsInModerationByIdResponse>>;

