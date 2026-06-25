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
        var dedupKey =
            $"comment:{message.RecipientUserId}:{message.ActorUserId}:{message.TargetType}:{message.TargetId}:{message.CommentId}";

        if (await notificationRepository.ExistsByDedupKeyAsync(dedupKey, ct))
            return;

        var notification = CommunityNotification.Create(
            message.RecipientUserId,
            DomainNotificationType.NewComment,
            "New comment",
            $"{message.ActorUserName} commented on your content.",
            message.ActorUserId,
            message.TargetType.ToString(),
            message.TargetId,
            commentId: message.CommentId,
            dedupKey: dedupKey);

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
        var dedupKey =
            $"reaction:{message.RecipientUserId}:{message.ActorUserId}:{message.TargetType}:{message.TargetId}:{message.ReactionType}";

        if (await notificationRepository.ExistsByDedupKeyAsync(dedupKey, ct))
            return;

        var reactionLabel = message.ReactionType switch
        {
            Contract.Enums.CommunityReactionType.WantToTry => "marked Want to try",
            Contract.Enums.CommunityReactionType.GreatFind => "marked Great find",
            Contract.Enums.CommunityReactionType.Helpful => "marked Helpful",
            _ => "reacted"
        };

        var notification = CommunityNotification.Create(
            message.RecipientUserId,
            DomainNotificationType.NewReaction,
            "New reaction",
            $"{message.ActorUserName} {reactionLabel} on your content.",
            message.ActorUserId,
            message.TargetType.ToString(),
            message.TargetId,
            reactionType: (int)message.ReactionType,
            dedupKey: dedupKey);

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
        var dedupKey = $"follow:{message.RecipientUserId}:{message.FollowerUserId}";

        if (await notificationRepository.ExistsByDedupKeyAsync(dedupKey, ct))
            return;

        var notification = CommunityNotification.Create(
            message.RecipientUserId,
            DomainNotificationType.NewFollower,
            "New follower",
            $"{message.FollowerUserName} started following you.",
            message.FollowerUserId,
            dedupKey: dedupKey);

        notificationRepository.Add(notification);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
