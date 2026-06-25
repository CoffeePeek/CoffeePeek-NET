using CoffeePeek.Account.Domain.Entities.CommunityNotificationAggregate;
using CoffeePeek.Contract.Events.Community;
using CoffeePeek.Shared.Kernel;
using DomainNotificationType = CoffeePeek.Account.Domain.Entities.CommunityNotificationAggregate.CommunityNotificationType;

namespace CoffeePeek.Account.Infrastructure.Consumers;

public class CommunityCommentNotificationHandler(
    ICommunityNotificationRepository notificationRepository,
    IUnitOfWork unitOfWork)
{
    public async Task Handle(CommunityCommentNotificationEvent message, CancellationToken ct)
    {
        var notification = CommunityNotification.Create(
            message.RecipientUserId,
            DomainNotificationType.NewComment,
            "New comment",
            $"{message.ActorUserName} commented on your content.",
            message.ActorUserId,
            message.TargetType.ToString(),
            message.TargetId);

        notificationRepository.Add(notification);
        await unitOfWork.SaveChangesAsync(ct);
    }
}

public class CommunityReactionNotificationHandler(
    ICommunityNotificationRepository notificationRepository,
    IUnitOfWork unitOfWork)
{
    public async Task Handle(CommunityReactionNotificationEvent message, CancellationToken ct)
    {
        var notification = CommunityNotification.Create(
            message.RecipientUserId,
            DomainNotificationType.NewReaction,
            "New reaction",
            $"{message.ActorUserName} reacted to your content.",
            message.ActorUserId,
            message.TargetType.ToString(),
            message.TargetId);

        notificationRepository.Add(notification);
        await unitOfWork.SaveChangesAsync(ct);
    }
}

public class CommunityFollowNotificationHandler(
    ICommunityNotificationRepository notificationRepository,
    IUnitOfWork unitOfWork)
{
    public async Task Handle(CommunityFollowNotificationEvent message, CancellationToken ct)
    {
        var notification = CommunityNotification.Create(
            message.RecipientUserId,
            DomainNotificationType.NewFollower,
            "New follower",
            $"{message.FollowerUserName} started following you.",
            message.FollowerUserId);

        notificationRepository.Add(notification);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
