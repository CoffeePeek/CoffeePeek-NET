using System.Text.Json.Serialization;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.CoffeeShop;
using MediatR;

namespace CoffeePeek.Contract.Requests.CoffeeShop;

public class GetAllReviewsRequest(Guid userId) : IRequest<Response<GetAllReviewsResponse>>
{
    [JsonIgnore]
    public Guid UserId { get; } = userId;
}