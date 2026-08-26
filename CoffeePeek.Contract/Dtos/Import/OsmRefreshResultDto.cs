namespace CoffeePeek.Contract.Dtos.Import;

public record OsmRefreshResultDto(
    int Fetched,
    int Inserted,
    int Updated,
    int CoffeeMapFetched = 0,
    int CoffeeMapInserted = 0,
    int CoffeeMapUpdated = 0);
