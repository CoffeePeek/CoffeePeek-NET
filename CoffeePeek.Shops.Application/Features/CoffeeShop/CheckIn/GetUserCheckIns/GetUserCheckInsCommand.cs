using System.ComponentModel.DataAnnotations;
using CoffeePeek.Contract.Abstract;
using MediatR;

namespace CoffeePeek.Shops.Application.Features.CoffeeShop.CheckIn.GetUserCheckIns;

public class GetUserCheckInsCommand(Guid userId, int pageNumber, int pageSize)
    : IRequest<Response<GetUserCheckInsResponse>>
{
    [Required]
    public Guid UserId { get; init; } = userId;
    [Range(1, int.MaxValue)]
    public int PageNumber { get; init; } = pageNumber;
    [Range(1, 100)]
    public int PageSize { get; init; } = pageSize;
}