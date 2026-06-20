using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Enums;
using CoffeePeek.Contract.Events.Moderation;
using CoffeePeek.Moderation.Domain.Aggregates.ModerationReviewAggregate;
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
        IMapper mapper,
        IUnitOfWork unitOfWork,
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
            {
                var reason = !string.IsNullOrWhiteSpace(command.Comment)
                    ? command.Comment
                    : command.RejectReason;
                review.Reject(reason!, command.UserId);
                break;
            }

            case ModerationStatus.Pending:
                review.MoveToPending(command.UserId);
                break;
        }

        await unitOfWork.SaveChangesAsync(ct);
        
        var response = UpdateEntityResponse<ModerationStatus>.Success((ModerationStatus)review.ModerationStatus, oldEntity: (ModerationStatus)oldStatus);
        
        return (response, approvedEvent);
    }
}