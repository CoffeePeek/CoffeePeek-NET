namespace CoffeePeek.Shared.Validation;

public class ValidationResult
{
    public bool IsValid => ErrorMessage == null;
    public string? ErrorMessage { get; init; }
    public static ValidationResult Valid => new();
    public static ValidationResult Invalid(string error) => new() { ErrorMessage = error };
}
