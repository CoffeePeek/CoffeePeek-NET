namespace CoffeePeek.AccountService.Extensions;

public static class CookiesExtension
{
    public const string RefreshToken = nameof(RefreshToken);

    public static string? GetRefreshToken(this IRequestCookieCollection cookies)
    {
        return cookies[RefreshToken];
    }
}