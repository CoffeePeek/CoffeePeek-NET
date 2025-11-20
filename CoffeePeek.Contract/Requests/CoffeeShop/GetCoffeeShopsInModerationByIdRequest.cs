using System.Text.Json.Serialization;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.CoffeeShop;
using MediatR;

namespace CoffeePeek.Contract.Requests.CoffeeShop;

public class GetCoffeeShopsInModerationByIdRequest(int userId) : IRequest<Response<GetCoffeeShopsInModerationByIdResponse>>
{
    [JsonIgnore] public int UserId { get; set; } = userId;
}