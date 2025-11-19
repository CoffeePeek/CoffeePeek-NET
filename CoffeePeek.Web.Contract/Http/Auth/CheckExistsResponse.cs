namespace CoffeePeek.Web.Contract.Http.Auth;

public class CheckExistsResponse
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
}