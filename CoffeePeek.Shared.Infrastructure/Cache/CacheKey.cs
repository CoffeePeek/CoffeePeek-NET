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
        
        public static string StatisticsPattern() => $"{nameof(User)}:statistics:*";
        
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
            DefaultTtl: TimeSpan.FromMinutes(3),
            Description: "Coffee shop entity (details)",
            Service: "ShopsService");
        
        public static string ByIdPattern() => $"{nameof(Shop)}:id:*";
        
        public static CacheKey ByCity(Guid cityId, int pageNumber, int pageSize) => new(
            Key: $"{nameof(Shop)}:city:{cityId}:page:{pageNumber}:size:{pageSize}",
            DefaultTtl: TimeSpan.FromMinutes(5),
            Description: "Coffee shops list by city with paging",
            Service: "ShopsService");
        
        public static string ByCityPattern(Guid cityId) => $"{nameof(Shop)}:city:{cityId}:*";
            
        public static CacheKey Cities() => new(
            Key: $"{nameof(Cities)}:all",
            DefaultTtl: TimeSpan.FromDays(1),
            Description: "All cities list",
            Service: "ShopsService");
        
        public static CacheKey Equipment() => new(
            Key: $"{nameof(Equipment)}:all",
            DefaultTtl: TimeSpan.FromDays(1),
            Description: "All equipment list",
            Service: "ShopsService");
        
        public static CacheKey Beans() => new(
            Key: $"{nameof(Beans)}:all",
            DefaultTtl: TimeSpan.FromDays(1),
            Description: "All beans list",
            Service: "ShopsService");
            
        public static CacheKey Favorites(Guid userId) => new(
            Key: $"{nameof(Shop)}:favorites:{userId}",
            DefaultTtl: TimeSpan.FromHours(1),
            Description: "User favorite shops",
            Service: "ShopsService");
            
        public static string AllPattern() => $"{nameof(Shop)}:*";
    }
    
    public static class Vacancies
    {
        public static CacheKey HHVacancies(string key) => new(
            Key: $"{nameof(Vacancies)}:key:{key}",
            DefaultTtl: TimeSpan.FromHours(1),
            Description: "Vacancies list hh.ru by key",
            Service: "JobVacancies");
        
        public static CacheKey GetAll(Guid cityId, int jobType, int page, int pageSize) => new(
            Key: $"{nameof(Vacancies)}:city:{cityId}:type:{jobType}:page:{page}:size:{pageSize}",
            DefaultTtl: TimeSpan.FromHours(1),
            Description: "Vacancies list by cityId, jobType, page and pageSize",
            Service: "JobVacancies");
        
        public static string AllPattern() => $"{nameof(Vacancies)}:*";
    }
    
    public static string AllPattern() => "*";
}

