using CoffeePeek.Account.Application.Features.DeleteUser;
using CoffeePeek.Account.Domain.Aggregates.UserAggregate;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Cache;
using MediatR;

namespace CoffeePeek.Account.Application.Features.User.DeleteUser;

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
        
        await ClearUserCacheAsync(user.Id);

        return Response<bool>.Success(true);
    }
    
    private Task ClearUserCacheAsync(Guid userId)
    {
        return Task.WhenAll(
            redisService.RemoveAsync(CacheKey.User.Profile(userId)), 
            redisService.RemoveAsync(CacheKey.User.Entity(userId))
            );
    }
}


