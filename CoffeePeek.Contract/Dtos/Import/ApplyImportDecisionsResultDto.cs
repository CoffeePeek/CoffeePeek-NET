namespace CoffeePeek.Contract.Dtos.Import;

public record ApplyImportDecisionsResultDto(
    int Applied,
    int Published,
    int Rejected,
    int Skipped,
    int Unknown,
    int Missing);
