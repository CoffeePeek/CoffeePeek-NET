using CoffeePeek.Infrastructure.Constants;
using CoffeePeek.Infrastructure.Services.Auth.Interfaces;
using Microsoft.AspNetCore.Http;

namespace CoffeePeek.Infrastructure.Services.Auth;

public class UserContextService(IHttpContextAccessor httpContextAccessor) : IUserContextService
{
    public bool TryGetUserId(out int userId)
    {
        userId = 0;
        var context = httpContextAccessor.HttpContext;
        return context?.Items != null 
               && context.Items.TryGetValue(AuthConfig.JWTTokenUserPropertyName, out var userIdObj) == true
               && userIdObj is int id
               && (userId = id) > 0;
    }
}