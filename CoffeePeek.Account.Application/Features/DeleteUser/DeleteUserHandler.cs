using CoffeePeek.Account.Domain.Aggregates.UserAggregate;
using CoffeePeek.Account.Domain.Repositories;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Cache;
using MediatR;

namespace CoffeePeek.Account.Application.Features.DeleteUser;

public class DeleteUserHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IRedisService redisService) 
    : IRequestHandler<DeleteUserCommand, Response<bool>>
{
    public async Task<Response<bool>> Handle(DeleteUserCommand command, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetById(command.UserId);

        if (user == null)
        {
            return Response<bool>.Error("User not found");
        }
        
        user.IsSoftDelete = true;
        
        await userRepository.Update(user);
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


