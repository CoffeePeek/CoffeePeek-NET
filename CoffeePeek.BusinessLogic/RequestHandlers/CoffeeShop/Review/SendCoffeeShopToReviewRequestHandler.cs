using CoffeePeek.Contract.Requests.CoffeeShop.Review;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.CoffeeShop.Review;
using CoffeePeek.Domain.Entities.Shop;
using CoffeePeek.Domain.Enums.Shop;
using CoffeePeek.Domain.UnitOfWork;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.BusinessLogic.RequestHandlers.CoffeeShop.Review;

internal class SendCoffeeShopToReviewRequestHandler(
    IMapper mapper, 
    IRepository<ReviewShop> reviewShopRepository)
    : IRequestHandler<SendCoffeeShopToReviewRequest, Response<SendCoffeeShopToReviewResponse>>
{
    public async Task<Response<SendCoffeeShopToReviewResponse>> Handle(SendCoffeeShopToReviewRequest request,
        CancellationToken cancellationToken)
    {
        // Проверяем, существует ли уже ReviewShop с таким же именем и адресом от этого пользователя
        // со статусом Pending (чтобы не блокировать уже обработанные заявки)
        var shopInReviewExists = await reviewShopRepository
            .FindBy(x => x.Name == request.Name && 
                         x.NotValidatedAddress == request.NotValidatedAddress &&
                         x.UserId == request.UserId &&
                         x.ReviewStatus == ReviewStatus.Pending)
            .AnyAsync(cancellationToken);

        if (shopInReviewExists)
        {
            return Response.ErrorResponse<Response<SendCoffeeShopToReviewResponse>>("A review submission with this name and address already exists.");
        }

        var reviewShop = mapper.Map<ReviewShop>(request);

        reviewShop.ReviewStatus = ReviewStatus.Pending;
        
        reviewShopRepository.Add(reviewShop);
        
        await reviewShopRepository.SaveChangesAsync(cancellationToken);
        
        return Response.SuccessResponse<Response<SendCoffeeShopToReviewResponse>>(null, "CoffeeShop added to review.");
    }
}