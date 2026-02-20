using CoffeePeek.Shared.Kernel.Exceptions;
using CoffeePeek.Shops.Domain.Entities;

namespace CoffeePeek.Shops.Domain.Aggregates.ReviewAggregate;

public sealed partial class Review
{
    public static Review Create(Guid shopId, Guid userId, string userName, string header,
        string comment, int ratingPlace, int ratingService, int ratingCoffee)
    {
        if (shopId == Guid.Empty)
            throw new DomainException($"{nameof(CoffeeShopId)} cannot be empty.");

        if (userId == Guid.Empty)
            throw new DomainException($"{nameof(UserId)} cannot be empty.");

        if (string.IsNullOrWhiteSpace(header))
            throw new DomainException("Review header is required.");

        if (header.Length is < BusinessConstants.MinReviewHeaderLength or > BusinessConstants.MaxReviewHeaderLength)
            throw new DomainException(
                $"{nameof(header)} must be between {BusinessConstants.MinReviewHeaderLength} and {BusinessConstants.MaxReviewHeaderLength} characters.");

        if (string.IsNullOrWhiteSpace(comment))
            throw new DomainException("Review comment is required.");

        if (comment.Length is < BusinessConstants.MinReviewCommentLength or > BusinessConstants.MaxReviewCommentLength)
            throw new DomainException(
                $"{nameof(comment)} must be between {BusinessConstants.MinReviewCommentLength} and {BusinessConstants.MaxReviewCommentLength} characters.");

        var rating = Rating.Create(ratingPlace, ratingService, ratingCoffee);
        
        return new Review(shopId, userId, userName, header, comment, rating);
    }

    public void UpdateUserName(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            throw new DomainException("UserName cannot be empty.");
        }
        
        if (userName == UserName)
        {
            return;
        }
        
        UserName = userName;
    }

    public void SoftDelete()
    {
        IsSoftDelete = true;
    }
}