using CoffeePeek.Account.Application.Features.DeleteUser;
using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Contract.Abstract;
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
        var user = await userRepository.GetById(command.UserId, cancellationToken);

        if (user == null)
        {
            return Response<bool>.Error("User not found");
        }

        user.SetSoftDelete();
        
        await userRepository.Update(user, cancellationToken);
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


