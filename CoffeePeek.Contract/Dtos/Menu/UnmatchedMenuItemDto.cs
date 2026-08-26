namespace CoffeePeek.Contract.Dtos.Menu;

/// <summary>Parser leftover for future custom drinks — not returned on public shop details.</summary>
public record UnmatchedMenuItemDto(string RawName, decimal? Price, double? Confidence);
