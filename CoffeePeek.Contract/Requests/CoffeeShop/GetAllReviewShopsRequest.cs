using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.CoffeeShop;
using MediatR;

namespace CoffeePeek.Contract.Requests.CoffeeShop;

public class GetAllReviewShopsRequest : IRequest<Response<GetCoffeeShopsInReviewByIdResponse>>
{
}

