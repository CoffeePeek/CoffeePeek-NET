namespace CoffeePeek.Client.App.Core.Identity;

public interface IUserRoleAccessor
{
    IReadOnlyList<string> GetRoles();

    bool IsInRole(string role);
}
