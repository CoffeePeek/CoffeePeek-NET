using CoffeePeek.Account.Application.Models;

namespace CoffeePeek.Account.Application.Services.Validation;

public interface IValidationStrategy<in TEntity>
{
    ValidationResult Validate(TEntity entity);
}