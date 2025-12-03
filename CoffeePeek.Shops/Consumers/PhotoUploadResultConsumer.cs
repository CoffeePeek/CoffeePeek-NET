using CoffeePeek.Domain.Repositories.Interfaces;
using CoffeePeek.Shared.Models.PhotoUpload;
using MassTransit;

namespace CoffeePeek.Shops.Consumers;

public class PhotoUploadResultConsumer(IModerationShopsRepository repository) : IConsumer<IPhotoUploadResult>
{
    public async Task Consume(ConsumeContext<IPhotoUploadResult> context)
    {
        await repository.UpdatePhotos(context.Message.ShopId, context.Message.UserId, context.Message.PhotoUrls);
    }
}