using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.Shops.Abstractions;

namespace CoffeePeek.BusinessLogic.Abstractions.Review;

public class ReviewUpdateValidationStrategy : BaseReviewValidationStrategy, IValidationStrategy<UpdateCoffeeShopReviewRequest>
{
    public ValidationResult Validate(UpdateCoffeeShopReviewRequest entity)
    {
        if (entity.ReviewId <= 0)
        {
            return ValidationResult.Invalid("ReviewId must be greater than 0");
        }

        var userIdValidation = ValidateUserId(entity.UserId);
        if (!userIdValidation.IsValid)
        {
            return userIdValidation;
        }

        var headerValidation = ValidateHeader(entity.Header);
        if (!headerValidation.IsValid)
        {
            return headerValidation;
        }

        var commentValidation = ValidateComment(entity.Comment);
        if (!commentValidation.IsValid)
        {
            return commentValidation;
        }

        if (!IsValidRating(entity.RatingCoffee))
        {
            return ValidationResult.Invalid($"RatingCoffee must be a valid number between {MinRating} and {MaxRating}");
        }

        if (!IsValidRating(entity.RatingService))
        {
            return ValidationResult.Invalid($"RatingService must be a valid number between {MinRating} and {MaxRating}");
        }

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (!IsValidRating(entity.RatingPlace))
        {
            return ValidationResult.Invalid($"RatingPlace must be a valid number between {MinRating} and {MaxRating}");
        }

        return ValidationResult.Valid;
    }
}
