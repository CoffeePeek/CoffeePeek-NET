using System.Text.Json.Serialization;
using CoffeePeek.Contract.Dtos.Public;

namespace CoffeePeek.Account.Application.Features.CommunityNotifications;

public record GetCommunityNotificationsQuery(int Page = 1, int PageSize = 20)
{
    [JsonIgnore] public Guid UserId { get; init; }
}

public record GetCommunityNotificationsResponse(
    IReadOnlyList<CommunityNotificationDto> Items,
    int TotalItems,
    int TotalPages,
    int CurrentPage,
    int PageSize,
    int UnreadCount);

public record MarkCommunityNotificationReadCommand(Guid NotificationId)
{
    [JsonIgnore] public Guid UserId { get; init; }
}

public record MarkAllCommunityNotificationsReadCommand
{
    [JsonIgnore] public Guid UserId { get; init; }
}
