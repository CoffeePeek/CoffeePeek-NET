namespace CoffeePeek.Shared.Auth;

public static class UserContextExtensions
{
    public static bool HasAnyRole(this IUserContext userContext, params string[] roles)
    {
        var userRoles = userContext.GetUserRole()?
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            ?? [];

        return roles.Any(role => userRoles.Contains(role, StringComparer.OrdinalIgnoreCase));
    }
}
