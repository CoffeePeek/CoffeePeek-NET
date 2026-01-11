using CoffeePeek.Shops.Domain;

namespace CoffeePeek.Shops.Application.ValidationStrategy.Review;

public abstract class BaseReviewValidationStrategy
{
    protected const int MinRating = BusinessConstants.MinReviewRate;
    protected const int MaxRating = BusinessConstants.MaxReviewRate;
    protected const int MinHeaderLength = BusinessConstants.MinReviewHeaderLength;
    protected const int MaxHeaderLength = BusinessConstants.MaxReviewHeaderLength;
    protected const int MinCommentLength = BusinessConstants.MinReviewCommentLength;
    protected const int MaxCommentLength = BusinessConstants.MaxReviewCommentLength;

    protected static ValidationResult ValidateUserId(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            return ValidationResult.Invalid("UserId must be not empty");
        }
        return ValidationResult.Valid;
    }

    protected static ValidationResult ValidateHeader(string header)
    {
        if (string.IsNullOrWhiteSpace(header))
        {
            return ValidationResult.Invalid("Header is required");
        }

        if (header.Length is < MinHeaderLength or > MaxHeaderLength)
        {
            return ValidationResult.Invalid($"Header must be between {MinHeaderLength} and {MaxHeaderLength} characters");
        }

        return ValidationResult.Valid;
    }

    protected static ValidationResult ValidateComment(string comment)
    {
        if (string.IsNullOrWhiteSpace(comment))
        {
            return ValidationResult.Invalid("Comment is required");
        }

        if (comment.Length is < MinCommentLength or > MaxCommentLength)
        {
            return ValidationResult.Invalid($"Comment must be between {MinCommentLength} and {MaxCommentLength} characters");
        }

        return ValidationResult.Valid;
    }

    protected static bool IsValidRating(int rating)
    {
        return rating is >= MinRating and <= MaxRating;
    }
}
