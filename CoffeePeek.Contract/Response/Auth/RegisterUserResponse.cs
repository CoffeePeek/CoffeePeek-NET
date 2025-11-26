namespace CoffeePeek.Contract.Response.Auth;

public class RegisterUserResponse(string email, string fullName)
{
    public string Email { get; set; } = email;
    public string FullName { get; set; } = fullName;
}