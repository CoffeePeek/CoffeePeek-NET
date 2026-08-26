using CoffeePeek.Shared.Domain.Entities;
using CoffeePeek.Shared.Domain.Places;
using CoffeePeek.Shared.Kernel.Exceptions;

namespace CoffeePeek.Moderation.Domain.Aggregates.ShopImportCandidateAggregate;

public sealed class ShopImportDuplicateSuggestion : Entity<Guid>
{
    public Guid LeftCandidateId { get; private set; }
    public Guid RightCandidateId { get; private set; }
    public int Score { get; private set; }
    public double DistanceMeters { get; private set; }
    public List<string> Reasons { get; private set; } = [];
    public ImportDuplicateStatus Status { get; private set; }
    public Guid? ReviewedByUserId { get; private set; }
    public DateTimeOffset? ReviewedAtUtc { get; private set; }

    // ReSharper disable once UnusedMember.Local
    private ShopImportDuplicateSuggestion()
    {
    }

    public static ShopImportDuplicateSuggestion Create(
        Guid candidateA,
        Guid candidateB,
        PlaceDuplicateHint hint)
    {
        if (candidateA == Guid.Empty || candidateB == Guid.Empty)
            throw new DomainException("Both candidates are required.");
        if (candidateA == candidateB)
            throw new DomainException("Cannot suggest a candidate as a duplicate of itself.");

        var (left, right) = Order(candidateA, candidateB);
        return new ShopImportDuplicateSuggestion
        {
            Id = Guid.NewGuid(),
            LeftCandidateId = left,
            RightCandidateId = right,
            Score = hint.Score,
            DistanceMeters = hint.DistanceMeters,
            Reasons = hint.Reasons.ToList(),
            Status = ImportDuplicateStatus.Pending
        };
    }

    public bool Involves(Guid candidateId) =>
        LeftCandidateId == candidateId || RightCandidateId == candidateId;

    public Guid OtherId(Guid candidateId) =>
        candidateId == LeftCandidateId ? RightCandidateId : LeftCandidateId;

    public void Confirm(Guid reviewerId, DateTimeOffset now) =>
        Decide(ImportDuplicateStatus.Confirmed, reviewerId, now);

    public void Reject(Guid reviewerId, DateTimeOffset now) =>
        Decide(ImportDuplicateStatus.Rejected, reviewerId, now);

    public void Refresh(PlaceDuplicateHint hint)
    {
        if (Status != ImportDuplicateStatus.Pending)
            return;

        Score = hint.Score;
        DistanceMeters = hint.DistanceMeters;
        Reasons = hint.Reasons.ToList();
    }

    private void Decide(ImportDuplicateStatus status, Guid reviewerId, DateTimeOffset now)
    {
        if (reviewerId == Guid.Empty)
            throw new DomainException("Reviewer is required.");
        if (Status != ImportDuplicateStatus.Pending)
            throw new DomainException("Suggestion is already decided.");

        Status = status;
        ReviewedByUserId = reviewerId;
        ReviewedAtUtc = now;
    }

    public static (Guid Left, Guid Right) Order(Guid a, Guid b) =>
        a.CompareTo(b) < 0 ? (a, b) : (b, a);
}
