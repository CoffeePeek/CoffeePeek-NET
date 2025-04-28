namespace CoffeePeek.Domain.Entities.Auth;

public class UserToken
{
    public int UserId { get; set; }
    public string LoginProvider { get; set; }
    public string Name { get; set; }
    public string Value { get; set; }
}