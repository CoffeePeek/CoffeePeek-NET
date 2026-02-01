using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CoffeePeek.Contract.Abstract;
using CoffeePeek.Shops.Application.Features.CheckIn.GetUserCheckIns;
using MediatR;

namespace CoffeePeek.Shops.Application.Features.CoffeeShop.CheckIn.GetUserCheckIns;

public record GetUserCheckInsCommand(
    [property: JsonIgnore] Guid UserId,
    [Range(1, int.MaxValue)] int PageNumber,
    [Range(1, 100)] int PageSize)
    : IRequest<Response<GetUserCheckInsResponse>>;
