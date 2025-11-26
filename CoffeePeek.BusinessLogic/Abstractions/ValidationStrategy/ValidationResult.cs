namespace CoffeePeek.BusinessLogic.Abstractions;

public class ValidationResult
{
    public bool IsValid => ErrorMessage == null;
    public string ErrorMessage { get; init; }
    public static ValidationResult Valid => new ValidationResult();
    public static ValidationResult Invalid(string error) => new ValidationResult { ErrorMessage = error };
}