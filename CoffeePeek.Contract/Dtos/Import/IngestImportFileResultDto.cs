namespace CoffeePeek.Contract.Dtos.Import;

public record IngestImportFileResultDto(
    int Parsed,
    int Inserted,
    int Enriched,
    int Unchanged,
    int Invalid,
    int SuggestedDuplicates);
