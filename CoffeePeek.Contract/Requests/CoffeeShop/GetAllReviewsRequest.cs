using System.Text.Json.Serialization;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.CoffeeShop;
using MediatR;

namespace CoffeePeek.Contract.Requests.CoffeeShop;

public class GetAllReviewsRequest(Guid userId, int pageNumber = 1, int pageSize = 10)
    : IRequest<Response<GetAllReviewsResponse>>
{
    [JsonIgnore]
    public Guid UserId { get; } = userId;

    public int PageNumber { get; } = pageNumber;
    public int PageSize { get; } = pageSize;
}