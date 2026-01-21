using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Moderation.Application.Abstractions;
using CoffeePeek.Moderation.Domain.Entities;
using CoffeePeek.Shared.Infrastructure.Abstract;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoffeePeek.Moderation.Application.Features.Shop.UpdateShop;

public class UpdateModerationCoffeeShopHandler(
    IModerationShopRepository repository,
    IUnitOfWork unitOfWork,
    IYandexGeocodingService geocodingService,
    IMapper mapper,
    ILogger<UpdateModerationCoffeeShopHandler> logger) 
    : IRequestHandler<UpdateModerationCoffeeShopCommand, UpdateEntityResponse<ModerationShopDto>>
{
    public async Task<UpdateEntityResponse<ModerationShopDto>> Handle(
        UpdateModerationCoffeeShopCommand command,
        CancellationToken ct)
    {
        var moderationShopDto = command.ModerationShopDto;
        var shop = await repository.GetByIdWithOutDetails(command.ModerationShopDto.Id, ct);

        if (shop == null || shop.UserId != command.UserId)
        {
            return UpdateEntityResponse<ModerationShopDto>.Error("Shop not found or access denied");
        }

        shop.UpdateInfo(moderationShopDto.Name, moderationShopDto.Description, moderationShopDto.PriceRange, moderationShopDto.CityId);

        if (moderationShopDto.Address != null && moderationShopDto.Address != shop.Location?.Address)
        {
            var geo = await geocodingService.GeocodeAsync(moderationShopDto.Address, ct);
            if (geo != null) shop.Location!.SetLocation(geo.Latitude, geo.Longitude, moderationShopDto.Address);
        }

        if (moderationShopDto.ShopContact != null)
        {
            shop.UpdateContacts(
                moderationShopDto.ShopContact.PhoneNumber, 
                moderationShopDto.ShopContact.InstagramLink, 
                moderationShopDto.ShopContact.Email, 
                moderationShopDto.ShopContact.SiteLink);
        }

        if (moderationShopDto.Schedules != null)
            shop.UpdateSchedules(moderationShopDto.Schedules.Select(s =>
                (s.DayOfWeek, s.Intervals.Select(i => (i.OpenTime, i.CloseTime)).ToList())));
        
        shop.UpdateRelations(
            moderationShopDto.EquipmentIds, 
            moderationShopDto.CoffeeBeanIds, 
            moderationShopDto.RoasterIds, 
            moderationShopDto.BrewMethodIds);

        await unitOfWork.SaveChangesAsync(ct);

        var result = mapper.Map<ModerationShopDto>(shop);

        logger.LogInformation("Shop {ShopId} updated by user {UserId}", shop.Id, command.UserId);

        return UpdateEntityResponse<ModerationShopDto>.Success(result, "Shop updated successfully");
    }
}