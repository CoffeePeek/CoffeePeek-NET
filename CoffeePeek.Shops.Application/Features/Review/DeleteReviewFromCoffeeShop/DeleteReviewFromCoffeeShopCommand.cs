using CoffeePeek.Contract.Abstract;
using MediatR;

namespace CoffeePeek.Shops.Application.Features.Review.DeleteReviewFromCoffeeShop;

public record DeleteReviewFromCoffeeShopCommand(Guid ReviewId) : IRequest<Response>;