using CoffeePeek.Shops.Application.Services;
using CoffeePeek.Shops.Application.ValidationStrategy.Review;

namespace CoffeePeek.Shops.Application.Features.CoffeeShop.CreateCoffeeShopReview;

public class CreateCoffeeShopReviewValidationStrategy : BaseReviewValidationStrategy,
    IValidationStrategy<CreateCoffeeShopReviewCommand>
{
    public ValidationResult Validate(CreateCoffeeShopReviewCommand entity)
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