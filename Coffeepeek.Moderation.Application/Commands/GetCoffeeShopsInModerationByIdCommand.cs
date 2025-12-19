using System.Text.Json.Serialization;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.CoffeeShop;
using MediatR;

namespace Coffeepeek.Moderation.Application.Commands;

public class GetCoffeeShopsInModerationByIdRequest(Guid userId) : IRequest<Response<GetCoffeeShopsInModerationByIdResponse>>
{
    [JsonIgnore] public Guid UserId { get; set; } = userId;
}