namespace CoffeePeek.Account.Application.Common.Interfaces;

public interface ISessionTerminationNotifier
{
    Task NotifyForceLogoutAsync(Guid userId, string reason, CancellationToken ct);
}

public static class SessionTerminationReasons
{
    public const string SessionRevoked = "session_revoked";
    public const string AllSessionsRevoked = "all_sessions_revoked";
    public const string UserBlocked = "user_blocked";
    public const string UserDeleted = "user_deleted";
    public const string PasswordChanged = "password_changed";
    public const string PasswordReset = "password_reset";
}
