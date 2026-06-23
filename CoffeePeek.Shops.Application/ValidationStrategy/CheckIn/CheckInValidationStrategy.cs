using CoffeePeek.Shared.Validation;
using CoffeePeek.Shops.Application.Features.CheckIn.CreateCheckIn;
using CoffeePeek.Shops.Domain;
using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;

namespace CoffeePeek.Shops.Application.ValidationStrategy.CheckIn;

public class CheckInValidationStrategy(
    IQueryCoffeeShopRepository queryCoffeeShopRepository)
    : IAsyncValidationStrategy<CreateCheckInCommand>
{
    public async Task<ValidationResult> ValidateAsync(CreateCheckInCommand command, CancellationToken ct)
    {
        var shopExists = await queryCoffeeShopRepository.Exists(command.CoffeeShopId, ct);
        if (!shopExists)
        {
            return ValidationResult.Invalid("Coffee shop not found");
        }

        return Validate(command);
    }

    private static ValidationResult Validate(CreateCheckInCommand command)
    {
        if (command.UserId == Guid.Empty)
        {
            return ValidationResult.Invalid("UserId is required");
        }

        if (command.CoffeeShopId == Guid.Empty)
        {
            return ValidationResult.Invalid("ShopId is required");
        }

        var visitedAtUtc = NormalizeToUtc(command.VisitedAt);
        var latestAllowedUtc = DateTime.UtcNow.AddMinutes(BusinessConstants.MaxVisitedAtClockSkewMinutes);
        if (visitedAtUtc > latestAllowedUtc)
        {
            return ValidationResult.Invalid("VisitedAt cannot be in the future");
        }

        if (!string.IsNullOrWhiteSpace(command.Note) && command.Note.Length > BusinessConstants.MaxCheckInNoteLength)
        {
            return ValidationResult.Invalid(
                $"Note must not exceed {BusinessConstants.MaxCheckInNoteLength} characters");
        }

        if (command.IsPublic)
        {
            if (command.Rating == null)
            {
                return ValidationResult.Invalid("Rating is required when checking in publicly");
            }

            if (command.Rating.Coffee < BusinessConstants.MinReviewRate || command.Rating.Coffee > BusinessConstants.MaxReviewRate
                                          || command.Rating.Place < BusinessConstants.MinReviewRate
                                          || command.Rating.Place > BusinessConstants.MaxReviewRate
                                          || command.Rating.Service < BusinessConstants.MinReviewRate
                                          || command.Rating.Service > BusinessConstants.MaxReviewRate)
            {
                return ValidationResult.Invalid($"Rating must be between {BusinessConstants.MinReviewRate} and {BusinessConstants.MaxReviewRate}");
            }

            if (command.Note == null)
            {
                return ValidationResult.Invalid("Note is required when checking in publicly");
            }

            if (command.Note.Length is > BusinessConstants.MaxCheckInNoteLength
                or < BusinessConstants.MinPublicCheckinNoteLength)
            {
                return ValidationResult.Invalid(
                    $"Note must be between {BusinessConstants.MinPublicCheckinNoteLength} and {BusinessConstants.MaxCheckInNoteLength} characters");
            }
        }

        return ValidationResult.Valid;
    }

    private static DateTime NormalizeToUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };
}
