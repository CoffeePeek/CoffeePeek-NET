namespace CoffeePeek.Contract.Enums;

/// <summary>Why an OSM import candidate was rejected. Values start at 1 (null = unknown / legacy).</summary>
public enum ImportRejectReason
{
    Closed = 1,
    Invalid = 2,
    NotCoffee = 3,
    Duplicate = 4
}
