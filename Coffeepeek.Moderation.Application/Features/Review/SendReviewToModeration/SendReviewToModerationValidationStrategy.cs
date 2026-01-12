using CoffeePeek.Moderation.Application.Features.Review.SendReviewToModeration;
using CoffeePeek.Shared.Validation;
using CoffeePeek.Shared.Validation.Review;

namespace Coffeepeek.Moderation.Application.Features.Review.SendReviewToModeration;

public class SendReviewToModerationValidationStrategy : BaseReviewValidationStrategy,
    IValidationStrategy<SendReviewToModerationCommand>
{
    public ValidationResult Validate(SendReviewToModerationCommand command)
    {
        var userIdValidation = ValidateUserId(command.UserId);
        if (!userIdValidation.IsValid)
        {
            return userIdValidation;
        }

        var headerValidation = ValidateHeader(command.Header);
        if (!headerValidation.IsValid)
        {
            return headerValidation;
        }

        var commentValidation = ValidateComment(command.Comment);
        if (!commentValidation.IsValid)
        {
            return commentValidation;
        }

        if (!IsValidRating(command.RatingCoffee))
        {
            return ValidationResult.Invalid($"RatingCoffee must be between {MinRating} and {MaxRating}");
        }

        if (!IsValidRating(command.RatingService))
        {
            return ValidationResult.Invalid($"RatingService must be between {MinRating} and {MaxRating}");
        }

        if (!IsValidRating(command.RatingPlace))
        {
            return ValidationResult.Invalid($"RatingPlace must be between {MinRating} and {MaxRating}");
        }

        return ValidationResult.Valid;
    }
}