using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Enums;
using CoffeePeek.Contract.Events.Moderation;
using CoffeePeek.Moderation.Application.Features.Admin.Audit;
using CoffeePeek.Moderation.Domain.Aggregates;
using CoffeePeek.Moderation.Domain.Aggregates.ModerationReviewAggregate;
using CoffeePeek.Moderation.Domain.Entities;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Exceptions;
using CoffeePeek.Shared.Kernel.Response;
using MapsterMapper;

namespace CoffeePeek.Moderation.Application.Features.Review.ChangeStatusModerationReview;

public static class ChangeStatusModerationReviewHandler
{
    public static async Task<(UpdateEntityResponse<ModerationStatus>, ModerationReviewApprovedEvent?)> Handle(
        ChangeStatusModerationReviewCommand command,
        IModerationReviewRepository repository,
        IModerationAuditLogRepository auditLogRepository,
        IMapper mapper,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        var review = await repository.GetById(command.ModerationReviewId, ct);

        if (review == null)
            throw new NotFoundException("Moderation review not found");

        ModerationReviewApprovedEvent? approvedEvent = null;

        var oldStatus = review.ModerationStatus;
        string? auditComment = null;

        switch (command.ModerationStatus)
        {
            case ModerationStatus.Approved:
                review.Approve(command.UserId);
                var dto = mapper.Map<ModerationReviewDto>(review);
                approvedEvent = new ModerationReviewApprovedEvent(dto);
                await ModerationAuditWriter.WriteAsync(
                    auditLogRepository,
                    ModerationAuditEntityType.Review,
                    review.Id,
                    review.Header,
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
                review.Reject(reason!, command.UserId);
                auditComment = reason;
                await ModerationAuditWriter.WriteAsync(
                    auditLogRepository,
                    ModerationAuditEntityType.Review,
                    review.Id,
                    review.Header,
                    ModerationAuditAction.Rejected,
                    command.UserId,
                    auditComment,
                    ct);
                break;
            }

            case ModerationStatus.Pending:
                review.MoveToPending(command.UserId);
                await ModerationAuditWriter.WriteAsync(
                    auditLogRepository,
                    ModerationAuditEntityType.Review,
                    review.Id,
                    review.Header,
                    ModerationAuditAction.Pending,
                    command.UserId,
                    null,
                    ct);
                break;
        }

        await unitOfWork.SaveChangesAsync(ct);
        
        var response = UpdateEntityResponse<ModerationStatus>.Success((ModerationStatus)review.ModerationStatus, oldEntity: (ModerationStatus)oldStatus);
        
        return (response, approvedEvent);
    }
}