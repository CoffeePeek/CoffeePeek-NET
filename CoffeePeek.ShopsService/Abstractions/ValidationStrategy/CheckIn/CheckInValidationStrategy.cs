using CoffeePeek.BusinessLogic.Abstractions;
using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.ShopsService.DB;

namespace CoffeePeek.ShopsService.Abstractions.ValidationStrategy.CheckIn;

public class CheckInValidationStrategy(ShopsDbContext dbContext) : IValidationStrategy<CreateCheckInRequest>
{
    private const int MaxNoteLength = 500;

    public ValidationResult Validate(CreateCheckInRequest entity)
    {
        if (entity.UserId == Guid.Empty)
        {
            return ValidationResult.Invalid("UserId is required");
        }

        if (entity.ShopId == Guid.Empty)
        {
            return ValidationResult.Invalid("ShopId is required");
        }

        if (!string.IsNullOrWhiteSpace(entity.Note) && entity.Note.Length > MaxNoteLength)
        {
            return ValidationResult.Invalid($"Note must not exceed {MaxNoteLength} characters");
        }

        // Проверка существования кофейни
        var shopExists = dbContext.Shops.Any(s => s.Id == entity.ShopId);
        if (!shopExists)
        {
            return ValidationResult.Invalid("Shop not found");
        }

        // Валидация Review, если он передан
        if (entity.Review != null)
        {
            if (string.IsNullOrWhiteSpace(entity.Review.Header))
            {
                return ValidationResult.Invalid("Review Header is required");
            }

            if (entity.Review.Header.Length is < 3 or > 70)
            {
                return ValidationResult.Invalid("Review Header must be between 3 and 70 characters");
            }

            if (string.IsNullOrWhiteSpace(entity.Review.Comment))
            {
                return ValidationResult.Invalid("Review Comment is required");
            }

            if (entity.Review.Comment.Length is < 10 or > 2000)
            {
                return ValidationResult.Invalid("Review Comment must be between 10 and 2000 characters");
            }

            if (entity.Review.RatingCoffee is < 1 or > 5)
            {
                return ValidationResult.Invalid("RatingCoffee must be between 1 and 5");
            }

            if (entity.Review.RatingPlace is < 1 or > 5)
            {
                return ValidationResult.Invalid("RatingPlace must be between 1 and 5");
            }

            if (entity.Review.RatingService is < 1 or > 5)
            {
                return ValidationResult.Invalid("RatingService must be between 1 and 5");
            }
        }

        return ValidationResult.Valid;
    }
}


