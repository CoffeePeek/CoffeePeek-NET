using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Responses;
using MediatR;

namespace Coffeepeek.Moderation.Application.UpdateShop;

public record UpdateModerationCoffeeShopCommand(ModerationShopDto ModerationShopDto, Guid UserId)
    : IRequest<UpdateEntityResponse<ModerationShopDto>>;