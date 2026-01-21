using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using MediatR;

namespace CoffeePeek.Moderation.Application.Features.Shop.UpdateShop;

public record UpdateModerationCoffeeShopCommand(ModerationShopDto ModerationShopDto, Guid UserId)
    : IRequest<UpdateEntityResponse<ModerationShopDto>>;