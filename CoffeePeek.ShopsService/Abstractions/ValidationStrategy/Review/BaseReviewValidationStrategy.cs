namespace CoffeePeek.BusinessLogic.Abstractions.Review;

public abstract class BaseReviewValidationStrategy
{
    protected const int MinRating = 1;
    protected const int MaxRating = 5;
    protected const int MinHeaderLength = 3;
    protected const int MaxHeaderLength = 100;
    protected const int MinCommentLength = 10;
    protected const int MaxCommentLength = 1000;

    protected static ValidationResult ValidateUserId(Guid userId)
    {
        if (userId.Variant > 0)
        {
            return ValidationResult.Invalid("UserId must be greater than 0");
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
