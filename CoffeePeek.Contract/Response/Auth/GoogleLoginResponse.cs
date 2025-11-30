using CoffeePeek.Contract.Dtos.Auth;

namespace CoffeePeek.Contract.Response.Auth;

public class GoogleLoginResponse
{
    public string AccessToken { get; init; }
    public string RefreshToken { get; init; }
    public GoogleLoginUserDto User { get; set; }
}