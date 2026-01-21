namespace CoffeePeek.Shared.Infrastructure.Constants;

public static class CapEventNames
{
    public static class Account
    {
        public const string UserNameChanged = "account.user.name.changed";
    }

    public static class Shops
    {
        public const string CheckinCreated = "shops.checkin.created";
        
        public const string ReviewAdded = "shops.review.added";
    }
    
    public static class Moderation
    {
        public const string ShopApproved = "moderation.shop.approved";
        public const string ReviewApproved = "moderation.review.approved";
        
        public static class CallBack
        {
            public const string ShopCompleted = "moderation.shop.callback.completed";
            public const string ReviewCompleted = "moderation.review.callback.completed";
        }
    }
}
