using CoffeePeek.Account.Domain.Entities.CommunityNotificationAggregate;
using CoffeePeek.Contract.Dtos.Public;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Exceptions;
using CoffeePeek.Shared.Kernel.Response;
using MapsterMapper;

namespace CoffeePeek.Account.Application.Features.CommunityNotifications;

public static class GetCommunityNotificationsHandler
{
    public static async Task<Response<GetCommunityNotificationsResponse>> Handle(
        GetCommunityNotificationsQuery query,
        IQueryCommunityNotificationRepository notificationRepository,
        IMapper mapper,
        CancellationToken ct)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 50);

        var (items, totalCount) = await notificationRepository.GetPageAsync(query.UserId, page, pageSize, ct);
        var unreadCount = await notificationRepository.GetUnreadCountAsync(query.UserId, ct);
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

        var dtos = items.Select(mapper.Map<CommunityNotificationDto>).ToList();

        return Response<GetCommunityNotificationsResponse>.Success(
            new GetCommunityNotificationsResponse(dtos, totalCount, totalPages, page, pageSize, unreadCount));
    }
}

public static class MarkCommunityNotificationReadHandler
{
    public static async Task<Response> Handle(
        MarkCommunityNotificationReadCommand command,
        ICommunityNotificationRepository notificationRepository,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        var notification = await notificationRepository.GetByIdForUserAsync(command.NotificationId, command.UserId, ct);
        if (notification is null)
            throw new NotFoundException("Notification not found.");

        if (!notification.IsRead)
        {
            notification.MarkRead();
            await unitOfWork.SaveChangesAsync(ct);
        }

        return Response.Success("Notification marked as read.");
    }
}

public static class MarkAllCommunityNotificationsReadHandler
{
    public static async Task<Response> Handle(
        MarkAllCommunityNotificationsReadCommand command,
        ICommunityNotificationRepository notificationRepository,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        await notificationRepository.MarkAllReadAsync(command.UserId, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return Response.Success("All notifications marked as read.");
    }
}
