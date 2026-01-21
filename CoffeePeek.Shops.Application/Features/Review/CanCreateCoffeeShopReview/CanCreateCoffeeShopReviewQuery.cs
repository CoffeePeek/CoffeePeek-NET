using CoffeePeek.Contract.Abstract;
using MediatR;

namespace CoffeePeek.Shops.Application.Features.Review.CanCreateCoffeeShopReview;

public record CanCreateCoffeeShopReviewQuery(Guid UserId, Guid ShopId) : IRequest<Response<CanCreateCoffeeShopReviewResponse>>;
