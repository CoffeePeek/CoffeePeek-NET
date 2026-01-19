using System.Security.Claims;

namespace CoffeePeek.Shared.Infrastructure;

public static class ClaimsPrincipalExtensions
{
    extension(ClaimsPrincipal user)
    {
        public Guid GetUserIdOrThrow()
        {
            ArgumentNullException.ThrowIfNull(user);

            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedAccessException("User ID claim is missing.");

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                throw new UnauthorizedAccessException("User ID claim is invalid or missing.");

            return userId;
        }
        
        public Guid? GetUserId()
        {
            ArgumentNullException.ThrowIfNull(user);

            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return null;
            }

            if (!Guid.TryParse(userIdClaim, out var userId))
                return null;

            return userId;
        }
        
        public string? GetUsername()
        {
            return user.FindFirst(ClaimTypes.Name)?.Value
                   ?? user.FindFirst("preferred_username")?.Value;
        }
        
        public string GetUsernameOrThrow()
        {
            var username = user.GetUsername();
            return string.IsNullOrEmpty(username) ? throw new UnauthorizedAccessException("Username claim is missing.") : username;
        }
    }
}