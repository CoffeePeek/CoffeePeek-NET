namespace CoffeePeek.Shared.Validation;

public interface IAsyncValidationStrategy<in TEntity>
{
    Task<ValidationResult> ValidateAsync(TEntity entity, CancellationToken ct);
}