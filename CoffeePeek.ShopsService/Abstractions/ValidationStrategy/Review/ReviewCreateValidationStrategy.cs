using CoffeePeek.BusinessLogic.Abstractions;
using CoffeePeek.BusinessLogic.Abstractions.Review;
using CoffeePeek.Contract.Requests.CoffeeShop;

namespace CoffeePeek.ShopsService.Abstractions.ValidationStrategy.Review;

public class ReviewCreateValidationStrategy : BaseReviewValidationStrategy, IValidationStrategy<AddCoffeeShopReviewRequest>
{
    public ValidationResult Validate(AddCoffeeShopReviewRequest entity)
    {
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
            return ValidationResult.Invalid($"RatingCoffee must be between {MinRating} and {MaxRating}");
        }

        if (!IsValidRating(entity.RatingService))
        {
            return ValidationResult.Invalid($"RatingService must be between {MinRating} and {MaxRating}");
        }

        if (!IsValidRating(entity.RatingPlace))
        {
            return ValidationResult.Invalid($"RatingPlace must be between {MinRating} and {MaxRating}");
        }

        return ValidationResult.Valid;
    }
}