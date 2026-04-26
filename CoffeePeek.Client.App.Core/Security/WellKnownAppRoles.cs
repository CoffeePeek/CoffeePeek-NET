namespace CoffeePeek.Client.App.Core.Security;

/// <summary>
/// Role names expected in JWT <c>role</c> claims and in server authorization policies.
/// Keep in sync with <c>CoffeePeek.Shared.Auth.Constants.RoleConsts.Moderator</c> (admin API / identity).
/// </summary>
public static class WellKnownAppRoles
{
    public const string Moderator = "Moderator";
}
