using CoffeePeek.Contract.Requests.CoffeeShop.Review;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.CoffeeShop.Review;
using CoffeePeek.Domain.Entities.Shop;
using CoffeePeek.Domain.Enums.Shop;
using CoffeePeek.Domain.UnitOfWork;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.BusinessLogic.RequestHandlers.CoffeeShop.Moderation;

internal class SendCoffeeShopToModerationRequestHandler(
    IMapper mapper, 
    IRepository<ModerationShop> reviewShopRepository)
    : IRequestHandler<SendCoffeeShopToModerationRequest, Response<SendCoffeeShopToModerationResponse>>
{
    public async Task<Response<SendCoffeeShopToModerationResponse>> Handle(SendCoffeeShopToModerationRequest request,
        CancellationToken cancellationToken)
    {
        var shopInReviewExists = await reviewShopRepository
            .FindBy(x => x.Name == request.Name && 
                         x.NotValidatedAddress == request.NotValidatedAddress &&
                         x.UserId == request.UserId &&
                         x.ModerationStatus == ModerationStatus.Pending)
            .AnyAsync(cancellationToken);

        if (shopInReviewExists)
        {
            return Response.ErrorResponse<Response<SendCoffeeShopToModerationResponse>>("A review submission with this name and address already exists.");
        }

        var reviewShop = mapper.Map<ModerationShop>(request);

        reviewShop.ModerationStatus = ModerationStatus.Pending;
        
        reviewShopRepository.Add(reviewShop);
        
        await reviewShopRepository.SaveChangesAsync(cancellationToken);
        
        return Response.SuccessResponse<Response<SendCoffeeShopToModerationResponse>>(null, "CoffeeShop added to moderation.");
    }
}