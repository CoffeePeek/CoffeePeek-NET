using System.Text.RegularExpressions;
using CoffeePeek.AuthService.Commands;
using CoffeePeek.AuthService.Models;

namespace CoffeePeek.AuthService.Services.Validation;

public partial class UserCreateValidationStrategy : IValidationStrategy<RegisterUserCommand>
{
    private const string EmailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9-]+\.[a-zA-Z]{2,}$";

    public ValidationResult Validate(RegisterUserCommand entity)
    {
        if (entity.Password.Length is > 30 or < 6)
        {
            return ValidationResult.Invalid("Password must be between 6 and 30 characters");
        }

        if (!EmailRegex().IsMatch(entity.Email))
        {
            return ValidationResult.Invalid("Invalid email address");
        }
        
        return ValidationResult.Valid;
    }

    [GeneratedRegex(EmailPattern)]
    private static partial Regex EmailRegex();
}