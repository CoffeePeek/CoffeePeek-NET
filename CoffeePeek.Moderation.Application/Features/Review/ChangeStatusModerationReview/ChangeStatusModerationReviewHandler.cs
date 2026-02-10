using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Constants;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Enums;
using CoffeePeek.Contract.Events;
using CoffeePeek.Contract.Events.Moderation;
using CoffeePeek.Moderation.Domain.Entities.ModerationReviewAggregate;
using CoffeePeek.Shared.Extensions.Exceptions;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Constants;
using DotNetCore.CAP;
using MapsterMapper;
using MediatR;

namespace CoffeePeek.Moderation.Application.Features.Review.ChangeStatusModerationReview;

public class ChangeStatusModerationReviewHandler(
    IModerationReviewRepository repository, 
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ICapPublisher capPublisher)
    : IRequestHandler<ChangeStatusModerationReviewCommand, UpdateEntityResponse<ModerationStatus>>
{
    public async Task<UpdateEntityResponse<ModerationStatus>> Handle(ChangeStatusModerationReviewCommand request,
        CancellationToken cancellationToken)
    {
        var moderationReview = await repository.GetById(request.ModerationReviewId, cancellationToken);

        if (moderationReview == null)
        {
            throw new NotFoundException("Moderation review not found");
        }

        switch (request.ModerationStatus)
        {
            case ModerationStatus.Approved:
                moderationReview.Approve(request.UserId);
                var reviewDto = mapper.Map<ModerationReviewDto>(moderationReview);
                await capPublisher.PublishAsync(
                    name: CapEventNames.Moderation.ReviewApproved,
                    contentObj: new ModerationReviewApprovedEvent(reviewDto),
                    cancellationToken: cancellationToken);
                break;
            case ModerationStatus.Rejected:
                moderationReview.Reject(request.RejectReason!, request.UserId);
                break;
            case ModerationStatus.Pending:
                moderationReview.MoveToPending(request.UserId);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return UpdateEntityResponse<ModerationStatus>.Success(moderationReview.ModerationStatus);
    }
}