using CoffeePeek.Contract.Requests.Auth;
using CoffeePeek.Contract.Response;
using CoffeePeek.Domain.Entities.Users;
using CoffeePeek.Infrastructure.Cache.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace CoffeePeek.BusinessLogic.RequestHandlers;

public class CheckUserExistsByEmailRequestHandler(UserManager<User> userManager,
    IRedisService redisService) 
    : IRequestHandler<CheckUserExistsByEmailRequest, Response>
{
    public async Task<Response> Handle(CheckUserExistsByEmailRequest request, CancellationToken cancellationToken)
    {
        var cacheKey = $"{nameof(User)}{request.Email}";
        var userFromRedis = await redisService.GetAsync<User>(cacheKey);

        if (userFromRedis != null)
        {
            return Response.SuccessResponse<Response>();
        }
        
        var user = await userManager.FindByEmailAsync(request.Email);

        if (user != null)
        {
            await redisService.SetAsync(cacheKey, user, TimeSpan.FromMinutes(5));
        }
        
        return user == null ? Response.ErrorResponse<Response>("User not found") : Response.SuccessResponse<Response>();
    }
}