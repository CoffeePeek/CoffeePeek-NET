using CoffeePeek.AuthService.Models;

namespace CoffeePeek.Auth.Application.Services.Validation;

public interface IValidationStrategy<in TEntity>
{
    ValidationResult Validate(TEntity entity);
}