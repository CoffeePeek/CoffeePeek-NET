using CoffeePeek.Contract.Requests.User;
using CoffeePeek.Contract.Response;
using CoffeePeek.Data.Interfaces;
using CoffeePeek.Shared.Infrastructure.Cache;
using CoffeePeek.Shared.Infrastructure.Interfaces.Redis;
using CoffeePeek.UserService.Repositories;
using MediatR;

namespace CoffeePeek.UserService.Handlers;

public class DeleteUserHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IRedisService redisService) 
    : IRequestHandler<DeleteUserRequest, Response<bool>>
{
    public async Task<Response<bool>> Handle(DeleteUserRequest request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId);

        if (user == null)
        {
            return Response<bool>.Error("User not found");
        }
        
        user.IsSoftDelete = true;
        
        await userRepository.UpdateAsync(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        await ClearUserCacheAsync(user.Id, user.Email);

        return Response<bool>.Success(true);
    }
    
    private async Task ClearUserCacheAsync(Guid userId, string email)
    {
        await redisService.RemoveAsync(CacheKey.User.Profile(userId));
        await redisService.RemoveAsync(CacheKey.User.ById(userId));
        await redisService.RemoveAsync(CacheKey.User.ByEmail(email));
    }
}


