using CoffeePeek.Account.Domain.Entities.CommunityNotificationAggregate;
using CoffeePeek.Account.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Account.Persistence.Repositories;

public class CommunityNotificationRepository(AccountDbContext dbContext) : ICommunityNotificationRepository
{
    public void Add(CommunityNotification notification) => dbContext.CommunityNotifications.Add(notification);

    public Task<CommunityNotification?> GetByIdForUserAsync(
        Guid notificationId,
        Guid userId,
        CancellationToken ct = default) =>
        dbContext.CommunityNotifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId, ct);

    public Task MarkAllReadAsync(Guid userId, CancellationToken ct = default) =>
        dbContext.CommunityNotifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(setters => setters.SetProperty(n => n.IsRead, true), ct);
}
