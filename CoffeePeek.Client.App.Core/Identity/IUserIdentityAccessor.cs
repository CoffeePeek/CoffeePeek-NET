namespace CoffeePeek.Client.App.Core.Identity;

public interface IUserIdentityAccessor
{
    Guid? GetCurrentUserIdOrNull();
}
