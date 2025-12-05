using System.Text.Json.Serialization;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.CoffeeShop;
using MediatR;

namespace CoffeePeek.Contract.Requests.CoffeeShop;

public class GetCoffeeShopsInModerationByIdRequest(Guid userId) : IRequest<Response<GetCoffeeShopsInModerationByIdResponse>>
{
    [JsonIgnore] public Guid UserId { get; set; } = userId;
}