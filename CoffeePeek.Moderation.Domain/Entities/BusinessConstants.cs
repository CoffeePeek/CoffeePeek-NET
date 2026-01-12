namespace CoffeePeek.Moderation.Domain.Entities;

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
}