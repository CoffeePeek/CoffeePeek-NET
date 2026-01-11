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
}