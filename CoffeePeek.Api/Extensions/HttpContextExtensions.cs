using CoffeePeek.Infrastructure.Constants;

namespace CoffeePeek.Api.Extensions;

public static class HttpContextExtensions
{
    /// <param name="context">The HTTP context</param>
    extension(HttpContext context)
    {
        /// <summary>
        /// Gets the authenticated user's ID from the HTTP context.
        /// This value is set by UserTokenMiddleware from the JWT token claims.
        /// </summary>
        /// <returns>The user ID if authenticated, otherwise null</returns>
        public int? GetUserId()
        {
            if (context.Items.TryGetValue(AuthConfig.JWTTokenUserPropertyName, out var userId))
            {
                return userId as int?;
            }

            return null;
        }

        /// <summary>
        /// Gets the authenticated user's ID from the HTTP context.
        /// Throws an exception if the user is not authenticated.
        /// </summary>
        /// <returns>The user ID</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when user is not authenticated</exception>
        public int GetUserIdOrThrow()
        {
            var userId = context.GetUserId();
            if (!userId.HasValue)
            {
                throw new UnauthorizedAccessException("User is not authenticated or UserId is not available.");
            }

            return userId.Value;
        }
    }
}