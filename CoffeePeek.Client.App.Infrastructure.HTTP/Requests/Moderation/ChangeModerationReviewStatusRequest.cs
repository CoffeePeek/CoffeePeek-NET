using CoffeePeek.Contract.Enums;

namespace CoffeePeek.Client.App.Infrastructure.HTTP.Requests.Moderation;

public sealed class ChangeModerationReviewStatusRequest
{
    public required Guid ModerationReviewId { get; init; }

    public required ModerationStatus ModerationStatus { get; init; }

    public string? RejectReason { get; init; }
}
