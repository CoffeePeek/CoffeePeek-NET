using CoffeePeek.Contract.Dtos.Public;
using CoffeePeek.Contract.Enums;
using CoffeePeek.Contract.Events.Moderation;
using CoffeePeek.Moderation.Application.Features.Admin.Audit;
using CoffeePeek.Moderation.Domain.Aggregates;
using CoffeePeek.Moderation.Domain.Aggregates.ModerationCommunityPostAggregate;
using CoffeePeek.Moderation.Domain.Entities;
using CoffeePeek.Moderation.Domain.Common.Enums;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Exceptions;
using CoffeePeek.Shared.Kernel.Response;
using MapsterMapper;
using Wolverine;
using ContractModerationStatus = CoffeePeek.Contract.Enums.ModerationStatus;
using DomainModerationStatus = CoffeePeek.Moderation.Domain.Common.Enums.ModerationStatus;

namespace CoffeePeek.Moderation.Application.Features.CommunityPost.ChangeStatusModerationCommunityPost;

public static class ChangeStatusModerationCommunityPostHandler
{
    public static async Task<(UpdateEntityResponse<ContractModerationStatus>, ModerationCommunityPostApprovedEvent?)> Handle(
        ChangeStatusModerationCommunityPostCommand command,
        IModerationCommunityPostRepository repository,
        IQueryModerationShopRepository moderationShopRepository,
        IModerationAuditLogRepository auditLogRepository,
        IMapper mapper,
        OutgoingMessages outgoingMessages,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        var post = await repository.GetById(command.ModerationCommunityPostId, ct);

        if (post is null)
            throw new NotFoundException("Moderation community post not found");

        ModerationCommunityPostApprovedEvent? approvedEvent = null;
        var oldStatus = post.ModerationStatus;
        string? auditComment = null;
        var wasApproved = oldStatus == DomainModerationStatus.Approved;

        switch (command.ModerationStatus)
        {
            case ContractModerationStatus.Approved:
            {
                if (post.LinkedShopId is { } linkedShopId)
                {
                    var moderationShop = await moderationShopRepository.GetByPublishedShopId(linkedShopId, ct);
                    if (moderationShop is null)
                        throw new ValidationException("Linked coffee shop no longer exists. Reject the post or remove the shop link before approval.");
                }

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
            }

            case ContractModerationStatus.Rejected:
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

            case ContractModerationStatus.Pending:
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

        if (wasApproved && command.ModerationStatus is ContractModerationStatus.Rejected or ContractModerationStatus.Pending)
            outgoingMessages.Add(new ModerationCommunityPostUnpublishedEvent(post.Id));

        await unitOfWork.SaveChangesAsync(ct);

        var response = UpdateEntityResponse<ContractModerationStatus>.Success(
            (ContractModerationStatus)post.ModerationStatus,
            oldEntity: (ContractModerationStatus)oldStatus);

        return (response, approvedEvent);
    }
}
