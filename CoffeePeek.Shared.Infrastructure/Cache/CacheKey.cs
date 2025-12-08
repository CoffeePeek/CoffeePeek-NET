namespace CoffeePeek.Shared.Infrastructure.Cache;

public record CacheKey(
    string Key,
    TimeSpan? DefaultTtl = null,
    string Description = "",
    string Service = "")
{
    public static class User
    {
        public static CacheKey Profile(Guid userId) => new(
            Key: $"{nameof(User)}:profile:{userId}",
            DefaultTtl: TimeSpan.FromHours(1),
            Description: "User profile DTO",
            Service: "UserService");
            
        public static CacheKey ById(Guid userId) => new(
            Key: $"{nameof(User)}:id:{userId}",
            DefaultTtl: TimeSpan.FromHours(1),
            Description: "User entity",
            Service: "UserService");
            
        public static CacheKey ByEmail(string email) => new(
            Key: $"{nameof(User)}:email:{email.ToLowerInvariant()}",
            DefaultTtl: TimeSpan.FromHours(1),
            Description: "User by email",
            Service: "UserService");
            
        public static string ProfilePattern() => $"{nameof(User)}:profile:*";
        
        public static string AllPattern() => $"{nameof(User)}:*";
    }
    
    public static class Auth
    {
        public static CacheKey Credentials(Guid userId) => new(
            Key: $"{nameof(Auth)}:credentials:{userId}",
            DefaultTtl: TimeSpan.FromMinutes(30),
            Description: "User credentials entity",
            Service: "AuthService");
            
        public static CacheKey CredentialsByEmail(string email) => new(
            Key: $"{nameof(Auth)}:credentials:email:{email.ToLowerInvariant()}",
            DefaultTtl: TimeSpan.FromMinutes(5),
            Description: "User credentials by email",
            Service: "AuthService");
            
        public static CacheKey RefreshToken(string token) => new(
            Key: $"{nameof(Auth)}:refresh-token:{token}",
            DefaultTtl: TimeSpan.FromDays(1),
            Description: "Refresh token",
            Service: "AuthService");
            
        public static string CredentialsPattern() => $"{nameof(Auth)}:credentials:*";
        
        public static string AllPattern() => $"{nameof(Auth)}:*";
    }
    
    public static class Shop
    {
        public static CacheKey ById(Guid shopId) => new(
            Key: $"{nameof(Shop)}:id:{shopId}",
            DefaultTtl: TimeSpan.FromHours(2),
            Description: "Coffee shop entity",
            Service: "ShopsService");
            
        public static CacheKey Cities() => new(
            Key: $"{nameof(Cities)}:all",
            DefaultTtl: TimeSpan.FromDays(1),
            Description: "All cities list",
            Service: "ShopsService");
            
        public static CacheKey Favorites(Guid userId) => new(
            Key: $"{nameof(Shop)}:favorites:{userId}",
            DefaultTtl: TimeSpan.FromHours(1),
            Description: "User favorite shops",
            Service: "ShopsService");
            
        public static string AllPattern() => $"{nameof(Shop)}:*";
    }
    
    public static string AllPattern() => "*";
}

