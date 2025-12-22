using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.Shops.Application;
using CoffeePeek.Shops.Application.Services;

namespace CoffeePeek.Shops.Infrastructure.ValidationStrategy.Review;

public class ReviewUpdateValidationStrategy : BaseReviewValidationStrategy, IValidationStrategy<UpdateCoffeeShopReviewRequest>
{
    public ValidationResult Validate(UpdateCoffeeShopReviewRequest entity)
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
