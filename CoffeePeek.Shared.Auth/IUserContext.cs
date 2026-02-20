namespace CoffeePeek.Shared.Auth;

public interface IUserContext
{
    Guid? GetUserId();
    Guid GetUserIdOrThrow();
    string? GetUsername();
    string GetUsernameOrThrow();
    string? GetUserRole();
    bool IsAuthenticated { get; }
}