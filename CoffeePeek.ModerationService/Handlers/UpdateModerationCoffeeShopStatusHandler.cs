using CoffeePeek.Contract.Dtos.Address;
using CoffeePeek.Contract.Dtos.Contact;
using CoffeePeek.Contract.Dtos.Schedule;
using CoffeePeek.Contract.Events.Moderation;
using CoffeePeek.Contract.Requests.CoffeeShop.Review;
using CoffeePeek.Domain.Enums.Shop;
using CoffeePeek.ModerationService.Repositories;
using MassTransit;
using MediatR;
using Response = CoffeePeek.Contract.Response.Response;

namespace CoffeePeek.ModerationService.Handlers;

public class UpdateModerationCoffeeShopStatusHandler(
    IModerationShopRepository repository,
    IPublishEndpoint publishEndpoint) 
    : IRequestHandler<UpdateModerationCoffeeShopStatusRequest, Response>
{
    public async Task<Response> Handle(UpdateModerationCoffeeShopStatusRequest request, CancellationToken cancellationToken)
    {
        var shop = await repository.GetByIdAsync(request.Id);

        if (shop == null)
        {
            return Response.ErrorResponse<Response>("CoffeeShop not found");
        }
        
        shop.ModerationStatus = request.ModerationStatus;

        // Если одобрено, публикуем событие
        if (request.ModerationStatus == ModerationStatus.Approved)
        {
            var addressDto = shop.Address != null ? new AddressDto
            {
                CityId = shop.Address.CityId,
                StreetId = shop.Address.StreetId,
                BuildingNumber = shop.Address.BuildingNumber,
                PostalCode = shop.Address.PostalCode,
                Latitude = shop.Address.Latitude,
                Longitude = shop.Address.Longitude
            } : null;

            var contactDto = shop.ShopContacts != null ? new ShopContactDto
            {
                PhoneNumber = shop.ShopContacts.PhoneNumber,
                InstagramLink = shop.ShopContacts.InstagramLink
            } : null;

            var photos = shop.ShopPhotos.Select(p => p.Url).ToList();
            var schedules = shop.Schedules.Select(s => new ScheduleDto
            {
                DayOfWeek = s.DayOfWeek,
                OpeningTime = s.OpeningTime,
                ClosingTime = s.ClosingTime
            }).ToList();

            var approvedEvent = new CoffeeShopApprovedEvent(
                shop.Id,
                shop.Name,
                shop.NotValidatedAddress,
                shop.UserId,
                shop.AddressId,
                shop.ShopContactId,
                shop.Status,
                addressDto,
                contactDto,
                photos,
                schedules
            );

            await publishEndpoint.Publish(approvedEvent, cancellationToken);
        }

        await repository.UpdateAsync(shop);
        await repository.SaveChangesAsync();
        
        return Response.SuccessResponse<Response>();
    }
}

