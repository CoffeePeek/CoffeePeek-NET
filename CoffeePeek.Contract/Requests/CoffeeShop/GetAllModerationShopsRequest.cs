using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.CoffeeShop;
using CoffeePeek.Contract.Responses;
using MediatR;

namespace CoffeePeek.Contract.Requests.CoffeeShop;

public class GetAllModerationShopsRequest : IRequest<Response<GetCoffeeShopsInModerationByIdResponse>>;

