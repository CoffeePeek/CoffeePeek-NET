namespace CoffeePeek.Moderation.Application.ErrorCodes;

/// <summary>Machine-readable error codes for the shop moderation flow.</summary>
public static class ShopModerationErrorCodes
{
    /// <summary>A shop with the same name and address is already in moderation or approved.</summary>
    public const string DuplicateShop = "SHOP_DUPLICATE";

    /// <summary>The provided shop name is invalid (too long, empty, etc.).</summary>
    public const string InvalidName = "SHOP_INVALID_NAME";

    /// <summary>The provided address is empty or malformed.</summary>
    public const string InvalidAddress = "SHOP_INVALID_ADDRESS";

    /// <summary>The referenced city was not found.</summary>
    public const string CityNotFound = "SHOP_CITY_NOT_FOUND";

    /// <summary>The moderation record was not found.</summary>
    public const string ModerationNotFound = "MODERATION_NOT_FOUND";

    /// <summary>The moderation record is in a state that does not allow this operation.</summary>
    public const string InvalidModerationStatus = "MODERATION_INVALID_STATUS";
}
