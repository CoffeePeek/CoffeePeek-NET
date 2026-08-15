namespace CoffeePeek.Contract.Responses;

public record ImportCandidatePublishResult(Guid CandidateId, Guid ShopId);

public record ImportCandidatePublishCompleteResponse(IReadOnlyList<ImportCandidatePublishResult> Results);
