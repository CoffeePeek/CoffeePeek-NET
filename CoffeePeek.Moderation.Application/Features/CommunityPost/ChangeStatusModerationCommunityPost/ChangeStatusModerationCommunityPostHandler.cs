using CoffeePeek.Contract.Dtos.Public;
using CoffeePeek.Contract.Enums;
using CoffeePeek.Contract.Events.Moderation;
using CoffeePeek.Moderation.Application.Features.Admin.Audit;
using CoffeePeek.Moderation.Domain.Aggregates;
using CoffeePeek.Moderation.Domain.Aggregates.ModerationCommunityPostAggregate;
using CoffeePeek.Moderation.Domain.Entities;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Exceptions;
using CoffeePeek.Shared.Kernel.Response;
using MapsterMapper;

namespace CoffeePeek.Moderation.Application.Features.CommunityPost.ChangeStatusModerationCommunityPost;

public static class ChangeStatusModerationCommunityPostHandler
{
    public static async Task<(UpdateEntityResponse<ModerationStatus>, ModerationCommunityPostApprovedEvent?)> Handle(
        ChangeStatusModerationCommunityPostCommand command,
        IModerationCommunityPostRepository repository,
        IModerationAuditLogRepository auditLogRepository,
        IMapper mapper,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        var post = await repository.GetById(command.ModerationCommunityPostId, ct);

        if (post is null)
            throw new NotFoundException("Moderation community post not found");

        ModerationCommunityPostApprovedEvent? approvedEvent = null;
        var oldStatus = post.ModerationStatus;
        string? auditComment = null;

        switch (command.ModerationStatus)
        {
            case ModerationStatus.Approved:
                post.Approve(command.UserId);
                approvedEvent = new ModerationCommunityPostApprovedEvent(mapper.Map<ModerationCommunityPostDto>(post));
                await ModerationAuditWriter.WriteAsync(
                    auditLogRepository,
                    ModerationAuditEntityType.CommunityPost,
                    post.Id,
                    post.Title,
                    ModerationAuditAction.Approved,
                    command.UserId,
                    null,
                    ct);
                break;

            case ModerationStatus.Rejected:
            {
                var reason = !string.IsNullOrWhiteSpace(command.Comment)
                    ? command.Comment.Trim()
                    : command.RejectReason;
                post.Reject(reason!, command.UserId);
                auditComment = reason;
                await ModerationAuditWriter.WriteAsync(
                    auditLogRepository,
                    ModerationAuditEntityType.CommunityPost,
                    post.Id,
                    post.Title,
                    ModerationAuditAction.Rejected,
                    command.UserId,
                    auditComment,
                    ct);
                break;
            }

            case ModerationStatus.Pending:
                post.MoveToPending(command.UserId);
                await ModerationAuditWriter.WriteAsync(
                    auditLogRepository,
                    ModerationAuditEntityType.CommunityPost,
                    post.Id,
                    post.Title,
                    ModerationAuditAction.Pending,
                    command.UserId,
                    null,
                    ct);
                break;
        }

        await unitOfWork.SaveChangesAsync(ct);

        var response = UpdateEntityResponse<ModerationStatus>.Success(
            (ModerationStatus)post.ModerationStatus,
            oldEntity: (ModerationStatus)oldStatus);

        return (response, approvedEvent);
    }
}
