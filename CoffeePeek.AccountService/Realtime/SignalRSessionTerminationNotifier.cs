using CoffeePeek.Account.Application.Common.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace CoffeePeek.AccountService.Realtime;

public class SignalRSessionTerminationNotifier(
    IHubContext<SessionHub, ISessionHubClient> hubContext,
    ILogger<SignalRSessionTerminationNotifier> logger) : ISessionTerminationNotifier
{
    public async Task NotifyForceLogoutAsync(Guid userId, string reason, CancellationToken ct)
    {
        try
        {
            await hubContext.Clients.User(userId.ToString())
                .ForceLogout(new ForceLogoutPayload(reason, DateTime.UtcNow));
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send force-logout SignalR event for user {UserId}.", userId);
        }
    }
}
