using CoffeePeek.Contract.Enums;
using CoffeePeek.Contract.Requests.CoffeeShop.Review;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.CoffeeShop.Review;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Data.Interfaces;
using CoffeePeek.ModerationService.Models;
using CoffeePeek.ModerationService.Repositories;
using CoffeePeek.ModerationService.Repositories.Interfaces;
using CoffeePeek.ModerationService.Services.Interfaces;
using MediatR;

namespace CoffeePeek.ModerationService.Handlers;

public class SendCoffeeShopToModerationHandler(
    IModerationShopRepository repository,
    IUnitOfWork unitOfWork,
    IYandexGeocodingService geocodingService) 
    : IRequestHandler<SendCoffeeShopToModerationRequest, Response<SendCoffeeShopToModerationResponse>>
{
    /// <summary>
    /// Обрабатывает запрос на отправку кофейни на модерацию и создаёт соответствующую запись модерации.
    /// </summary>
    /// <param name="request">Запрос с данными кофейни (название, адрес для проверки и идентификатор пользователя).</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>`Response&lt;SendCoffeeShopToModerationResponse&gt;`: в случае успеха — успешный ответ с null в полезной нагрузке и сообщением о добавлении в модерацию; в случае дубликата — ошибочный ответ с сообщением о существующей заявке.</returns>
    public async Task<Response<SendCoffeeShopToModerationResponse>> Handle(SendCoffeeShopToModerationRequest request,
        CancellationToken cancellationToken)
    {
        var existingShop = await repository.GetByNameAndAddressAsync(request.Name, request.NotValidatedAddress, request.UserId);

        if (existingShop != null)
        {
            return Response<SendCoffeeShopToModerationResponse>.Error("A moderation submission with this name and address already exists.");
        }

        // Attempt geocoding
        var geocodingResult = await geocodingService.GeocodeAsync(request.NotValidatedAddress, cancellationToken);

        var moderationShop = new ModerationShop
        {
            Name = request.Name,
            NotValidatedAddress = request.NotValidatedAddress,
            UserId = request.UserId,
            ModerationStatus = ModerationStatus.Pending,
            Status = ShopStatus.NotConfirmed,
            IsAddressValidated = geocodingResult != null,
            Latitude = geocodingResult?.Latitude,
            Longitude = geocodingResult?.Longitude
        };

        await repository.AddAsync(moderationShop);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Response<SendCoffeeShopToModerationResponse>.Success(null, "CoffeeShop added to moderation.");
    }
}

