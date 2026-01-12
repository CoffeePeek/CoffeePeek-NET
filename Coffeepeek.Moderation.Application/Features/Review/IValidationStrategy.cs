namespace Coffeepeek.Moderation.Application.Features.Review;

public interface IValidationStrategy<in TEntity>
{
    ValidationResult Validate(TEntity entity);
}