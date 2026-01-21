namespace CoffeePeek.Shops.Domain;

public static class BusinessConstants
{
    #region Review
    
    public const int MinReviewRate = 1;
    public const int MaxReviewRate = 5;
    
    public const int MinReviewHeaderLength = 3;
    public const int MaxReviewHeaderLength = 100;
    public const int MinReviewCommentLength = 10;
    public const int MaxReviewCommentLength = 1000;
    
    #endregion 
    
    #region CheckIn
    
    public const int MaxCheckInNoteLength = 500;
    
    #endregion

    #region ShopContact

    public const int MaxShopContactInstagramLinkLength = 255;
    public const int MaxShopContactEmailLength = 255;
    public const int MaxShopContactSiteLinkLength = 2048;
    public const int MaxShopContactPhoneNumberLength = 20;

    #endregion

    #region CoffeeShop

    public const int ItNewEntityInDays = 30;

    public const int MaxCoffeeShopNameLength = 100;
    public const int MaxCoffeeShopDescriptionLength = 500;

    #endregion

    #region Visits

    public const int MaxVisitNoteLength = 500;

    #endregion

    #region Location

    public const int MaxLocationAddressLength = 500;
    public const int MaxLocationPrecision = 18;
    public const int MaxLocationScale = 10;
    public const int MaxLocationLatitude = 90;
    public const int MaxLocationLongitude = 180;

    #endregion
}