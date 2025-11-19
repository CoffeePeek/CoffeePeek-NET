using CoffeePeek.Contract.Requests.CoffeeShop;

namespace CoffeePeek.BusinessLogic.Abstractions.Review;

public class ReviewCreateValidationStrategy : IValidationStrategy<AddCoffeeShopReviewRequest>
{
    private const int MinRating = 1;
    private const int MaxRating = 5;
    private const int MinHeaderLength = 3;
    private const int MaxHeaderLength = 100;
    private const int MinCommentLength = 10;
    private const int MaxCommentLength = 1000;

    public ValidationResult Validate(AddCoffeeShopReviewRequest entity)
    {
        if (entity.ShopId <= 0)
        {
            return ValidationResult.Invalid("ShopId must be greater than 0");
        }

        if (entity.UserId <= 0)
        {
            return ValidationResult.Invalid("UserId must be greater than 0");
        }

        if (string.IsNullOrWhiteSpace(entity.Header))
        {
            return ValidationResult.Invalid("Header is required");
        }

        if (entity.Header.Length is < MinHeaderLength or > MaxHeaderLength)
        {
            return ValidationResult.Invalid(
                $"Header must be between {MinHeaderLength} and {MaxHeaderLength} characters");
        }

        if (string.IsNullOrWhiteSpace(entity.Comment))
        {
            return ValidationResult.Invalid("Comment is required");
        }

        if (entity.Comment.Length is < MinCommentLength or > MaxCommentLength)
        {
            return ValidationResult.Invalid(
                $"Comment must be between {MinCommentLength} and {MaxCommentLength} characters");
        }

        if (!IsValidRating(entity.RatingCoffee))
        {
            return ValidationResult.Invalid($"RatingCoffee must be between {MinRating} and {MaxRating}");
        }

        if (!IsValidRating(entity.RatingService))
        {
            return ValidationResult.Invalid($"RatingService must be between {MinRating} and {MaxRating}");
        }

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (!IsValidRating(entity.RatingPlace))
        {
            return ValidationResult.Invalid($"RatingPlace must be between {MinRating} and {MaxRating}");
        }

        return ValidationResult.Valid;
    }

    private static bool IsValidRating(int rating)
    {
        return rating is >= MinRating and <= MaxRating;
    }
}