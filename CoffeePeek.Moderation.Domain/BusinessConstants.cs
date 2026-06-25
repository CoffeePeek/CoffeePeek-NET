namespace CoffeePeek.Moderation.Domain;

public static class BusinessConstants
{
    public const int MinReviewRate = 1;
    public const int MaxReviewRate = 5;
    
    public const int MinReviewHeaderLength = 3;
    public const int MaxReviewHeaderLength = 100;
    public const int MinReviewCommentLength = 10;
    public const int MaxReviewCommentLength = 1000;
    public const int MinRejectReasonCommentLength = 2;
    public const int MaxRejectReasonCommentLength = 1000;

    public const int MinCommunityPostTitleLength = 3;
    public const int MaxCommunityPostTitleLength = 120;
    public const int MinCommunityPostBodyLength = 10;
    public const int MaxCommunityPostBodyLength = 2000;
    public const int MaxCommunityPostsPerUserPerDay = 10;
    
    #region ShopContact

    public const int MaxShopContactInstagramLinkLength = 255;
    public const int MaxShopContactEmailLength = 255;
    public const int MaxShopContactSiteLinkLength = 2048;
    public const int MaxShopContactPhoneNumberLength = 20;

    #endregion
}