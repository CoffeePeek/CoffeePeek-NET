using CoffeePeek.Contract.Responses;
using CoffeePeek.Moderation.Domain.Entities;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Constants;
using DotNetCore.CAP;

namespace CoffeePeek.Moderation.Infrastructure.Consumers;

public class ModerationShopApproveCompleteHandler(IModerationShopRepository repository, IUnitOfWork unitOfWork) : ICapSubscribe
{
    [CapSubscribe(CapEventNames.Moderation.CallBack.ShopCompleted)]
    public async Task Handle(ModerationShopApproveCompleteResponse @event, CancellationToken cancellationToken)
    {
        var moderationShop = await repository.GetByIdWithOutDetails(@event.ModerationShopId, cancellationToken);

        if (moderationShop == null) 
            return;
        
        moderationShop.AddShopId(@event.ShopId);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}