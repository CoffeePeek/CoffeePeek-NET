using System.Security.Claims;

namespace CoffeePeek.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    extension(ClaimsPrincipal user)
    {
        public int GetUserIdOrThrow()
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedAccessException("User ID claim is missing.");

            if (!int.TryParse(userIdClaim, out var userId))
                throw new UnauthorizedAccessException("User ID claim is invalid.");

            return userId;
        }
    }
}