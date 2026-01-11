using CoffeePeek.Shared.Extensions.Exceptions;

namespace CoffeePeek.Shops.Domain.Entities.ReviewAggregate;

public sealed partial class Review
{
    public static Review Create(Guid shopId, Guid userId, string header,
        string comment, int ratingCoffee, int ratingPlace, int ratingService)
    {
        if (shopId == Guid.Empty)
            throw new DomainException($"{nameof(ShopId)} cannot be empty.");

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

        if (ratingCoffee is < BusinessConstants.MinReviewRate or > BusinessConstants.MaxReviewRate)
            throw new DomainException(
                $"{nameof(ratingCoffee)} must be between {BusinessConstants.MinReviewRate} and {BusinessConstants.MaxReviewRate}.");

        if (ratingPlace is < BusinessConstants.MinReviewRate or > BusinessConstants.MaxReviewRate)
            throw new DomainException(
                $"{nameof(ratingPlace)} must be between {BusinessConstants.MinReviewRate} and {BusinessConstants.MaxReviewRate}.");

        if (ratingService is < BusinessConstants.MinReviewRate or > BusinessConstants.MaxReviewRate)
            throw new DomainException(
                $"{nameof(ratingService)} must be between {BusinessConstants.MinReviewRate} and {BusinessConstants.MaxReviewRate}.");

        return new Review(shopId, userId, header, comment, ratingCoffee, ratingPlace, ratingService);
    }

    public void UpdateHeader(string header)
    {
        if (header == Header)
        {
            return;
        }
        
        if (header.Length is < BusinessConstants.MinReviewHeaderLength or > BusinessConstants.MaxReviewHeaderLength)
        {
            throw new DomainException(
                $"{nameof(header)} header must be between {BusinessConstants.MinReviewHeaderLength} and {BusinessConstants.MaxReviewHeaderLength} characters.");
        }
        
        Header = header;
    }

    public void UpdateComment(string comment)
    {
        if (comment.Length is < BusinessConstants.MinReviewCommentLength or > BusinessConstants.MaxReviewCommentLength)
        {
            throw new DomainException(
                $"{nameof(comment)} must be between {BusinessConstants.MinReviewCommentLength} and {BusinessConstants.MaxReviewCommentLength} characters.");
        }
        
        Comment = comment;
    }

    public void UpdateRating(int ratingCoffee, int ratingPlace, int ratingService)
    {
        if (ratingCoffee is < BusinessConstants.MinReviewRate or > BusinessConstants.MaxReviewRate)
            throw new DomainException(
                $"{nameof(ratingCoffee)} must be between {BusinessConstants.MinReviewRate} and {BusinessConstants.MaxReviewRate}.");

        if (ratingPlace is < BusinessConstants.MinReviewRate or > BusinessConstants.MaxReviewRate)
            throw new DomainException(
                $"{nameof(ratingPlace)} must be between {BusinessConstants.MinReviewRate} and {BusinessConstants.MaxReviewRate}.");

        if (ratingService is < BusinessConstants.MinReviewRate or > BusinessConstants.MaxReviewRate)
            throw new DomainException(
                $"{nameof(ratingService)} must be between {BusinessConstants.MinReviewRate} and {BusinessConstants.MaxReviewRate}.");

        RatingCoffee = ratingCoffee;
        RatingPlace = ratingPlace;
        RatingService = ratingService;
    }

    public void SoftDelete()
    {
        IsSoftDelete = true;
    }
}