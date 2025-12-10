using CoffeePeek.Contract.Requests.User;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.User;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Data.Interfaces;
using CoffeePeek.Shared.Infrastructure.Cache;
using CoffeePeek.Shared.Infrastructure.Interfaces.Redis;
using CoffeePeek.UserService.Repositories;
using MediatR;

namespace CoffeePeek.UserService.Handlers;

public class UpdateProfileHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork, 
    IRedisService redisService)
    : IRequestHandler<UpdateProfileRequest, Response<UpdateProfileResponse>>
{
    public async Task<Response<UpdateProfileResponse>> Handle(UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId);
        
        if (user == null)
        {
            return Response<UpdateProfileResponse>.Error("User not found");
        }

        if (!string.IsNullOrWhiteSpace(request.UserName))
        {
            user.Username = request.UserName.Trim();
        }

        if (request.About is not null)
        {
            user.About = request.About;
        }

        var oldEmail = user.Email;
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            user.Email = request.Email.Trim();
        }
        
        await userRepository.UpdateAsync(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        await ClearUserCacheAsync(request.UserId, oldEmail);
        
        await redisService.SetAsync(CacheKey.User.ById(request.UserId), user);
        await redisService.SetAsync(CacheKey.User.ByEmail(user.Email), user);

        return Response<UpdateProfileResponse>.Success(new UpdateProfileResponse(), "Profile updated successfully");
    }
    
    private async Task ClearUserCacheAsync(Guid userId, string email)
    {
        await redisService.RemoveAsync(CacheKey.User.Profile(userId));
        await redisService.RemoveAsync(CacheKey.User.ById(userId));
        await redisService.RemoveAsync(CacheKey.User.ByEmail(email));
    }
}


