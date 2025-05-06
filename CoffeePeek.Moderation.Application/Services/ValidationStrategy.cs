namespace CoffeePeek.Moderation.Application.Services;

/// <summary>
/// CP-33 refactoring 
/// </summary>
public class ValidationStrategy
{
    public bool ValidateUserRegister(string email, string password)
    {
        return true;
    }
}