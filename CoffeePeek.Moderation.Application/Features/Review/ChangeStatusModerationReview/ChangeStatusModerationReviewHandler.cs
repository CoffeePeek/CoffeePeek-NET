using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Enums;
using CoffeePeek.Contract.Events.Moderation;
using CoffeePeek.Moderation.Domain.Aggregates.ModerationReviewAggregate;
using CoffeePeek.Shared.Kernel.Exceptions;
using CoffeePeek.Shared.Kernel.Response;
using MapsterMapper;
using Wolverine.Attributes;

namespace CoffeePeek.Moderation.Application.Features.Review.ChangeStatusModerationReview;

public static class ChangeStatusModerationReviewHandler
{
    [Transactional]
    public static async Task<(UpdateEntityResponse<ModerationStatus>, ModerationReviewApprovedEvent?)> Handle(
        ChangeStatusModerationReviewCommand command,
        IModerationReviewRepository repository,
        IMapper mapper,
        CancellationToken ct)
    {
        var review = await repository.GetById(command.ModerationReviewId, ct);

        if (review == null)
            throw new NotFoundException("Moderation review not found");

        ModerationReviewApprovedEvent? approvedEvent = null;

        var oldStatus = review.ModerationStatus;
        switch (command.ModerationStatus)
        {
            case ModerationStatus.Approved:
                review.Approve(command.UserId);
                // Маппим в DTO и готовим событие к отправке
                var dto = mapper.Map<ModerationReviewDto>(review);
                approvedEvent = new ModerationReviewApprovedEvent(dto);
                break;

            case ModerationStatus.Rejected:
                review.Reject(command.RejectReason!, command.UserId);
                break;

            case ModerationStatus.Pending:
                review.MoveToPending(command.UserId);
                break;
        }

        var response = UpdateEntityResponse<ModerationStatus>.Success((ModerationStatus)review.ModerationStatus, oldEntity: (ModerationStatus)oldStatus);
        
        return (response, approvedEvent);
    }
}