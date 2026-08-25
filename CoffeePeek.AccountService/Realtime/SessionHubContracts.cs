namespace CoffeePeek.AccountService.Realtime;

public sealed record ForceLogoutPayload(string Reason, DateTime OccurredAtUtc);

public interface ISessionHubClient
{
    Task ForceLogout(ForceLogoutPayload payload);
}
