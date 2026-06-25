namespace CoffeePeek.Account.Domain.Entities.CommunityNotificationAggregate;

public interface ICommunityNotificationRepository
{
    void Add(CommunityNotification notification);
    Task<CommunityNotification?> GetByIdForUserAsync(Guid notificationId, Guid userId, CancellationToken ct = default);
    Task MarkAllReadAsync(Guid userId, CancellationToken ct = default);
}

public interface IQueryCommunityNotificationRepository
{
    Task<(CommunityNotification[] Items, int TotalCount)> GetPageAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken ct = default);

    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct = default);
}
