using CoffeePeek.Moderation.Domain.Entities;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Validation;
using CoffeePeek.Shared.Validation.Review;

namespace CoffeePeek.Moderation.Application.Features.Review.SendReviewToModeration;

public class SendReviewToModerationValidationStrategy(IGenericRepository<ModerationShop> shopRepository) : BaseReviewValidationStrategy,
    IAsyncValidationStrategy<SendReviewToModerationCommand>
{
    public async Task<ValidationResult> ValidateAsync(SendReviewToModerationCommand command, CancellationToken ct = default)
    {
        var syncResult = ValidateSyncProps(command);
        if (!syncResult.IsValid) 
            return syncResult;

        var shop = await shopRepository.FirstOrDefaultAsNoTrackingAsync(x => x.ShopId == command.ShopId, ct);

        if (shop == null)
        {
            return ValidationResult.Invalid("Coffee shop not found");
        }

        return ValidationResult.Valid;
    }
    
    private static ValidationResult ValidateSyncProps(SendReviewToModerationCommand command)
    {
        var validations = new List<ValidationResult>
        {
            ValidateUserId(command.UserId),
            ValidateHeader(command.Header),
            ValidateComment(command.Comment)
        };

        var firstError = validations.FirstOrDefault(v => !v.IsValid);
        if (firstError != null) return firstError;

        if (!IsValidRating(command.RatingCoffee)) return ValidationResult.Invalid("RatingCoffee invalid");
        if (!IsValidRating(command.RatingService)) return ValidationResult.Invalid("RatingService invalid");
        if (!IsValidRating(command.RatingPlace)) return ValidationResult.Invalid("RatingPlace invalid");

        return ValidationResult.Valid;
    }
}