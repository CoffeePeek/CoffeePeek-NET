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

            if (!Guid.TryParse(userIdClaim, out var userId))
                throw new UnauthorizedAccessException("User ID claim is invalid.");

            return userId;
        }
    }
}