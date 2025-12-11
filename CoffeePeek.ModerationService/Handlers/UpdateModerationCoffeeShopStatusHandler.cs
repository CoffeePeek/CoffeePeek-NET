using CoffeePeek.Contract.Dtos.Contact;
using CoffeePeek.Contract.Dtos.Schedule;
using CoffeePeek.Contract.Enums;
using CoffeePeek.Contract.Events.Moderation;
using CoffeePeek.Contract.Requests.CoffeeShop.Review;
using CoffeePeek.Data.Interfaces;
using CoffeePeek.ModerationService.Repositories.Interfaces;
using MassTransit;
using MediatR;
using Response = CoffeePeek.Contract.Response.Response;

namespace CoffeePeek.ModerationService.Handlers;

public class UpdateModerationCoffeeShopStatusHandler(
    IModerationShopRepository repository,
    IPublishEndpoint publishEndpoint,
    IUnitOfWork unitOfWork) 
    : IRequestHandler<UpdateModerationCoffeeShopStatusRequest, Response>
{
    /// <summary>
    /// Обновляет статус модерации кофейни, сохраняет изменения и при одобрении публикует событие о подтверждении.
    /// </summary>
    /// <param name="request">Запрос с идентификатором кофейни и новым значением ModerationStatus.</param>
    /// <param name="cancellationToken">Токен отмены для асинхронных операций.</param>
    /// <returns>Response, указывающий на результат операции. Если кофейня не найдена, возвращается ошибка с сообщением "CoffeeShop not found".</returns>
    public async Task<Response> Handle(UpdateModerationCoffeeShopStatusRequest request, CancellationToken cancellationToken)
    {
        var shop = await repository.GetByIdAsync(request.Id);

        if (shop == null)
        {
            return Response.Error("CoffeeShop not found");
        }
        
        shop.ModerationStatus = request.ModerationStatus;

        // Если одобрено, публикуем событие
        if (request.ModerationStatus == ModerationStatus.Approved)
        {
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
                shop.Address,
                shop.ShopContactId,
                shop.Status,
                contactDto,
                photos,
                schedules,
                shop.Latitude,
                shop.Longitude
            );

            await publishEndpoint.Publish(approvedEvent, cancellationToken);
        }

        await repository.UpdateAsync(shop);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        return Response.Success();
    }
}

