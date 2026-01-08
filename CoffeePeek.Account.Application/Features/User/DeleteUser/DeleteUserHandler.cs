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
    IHybridCache hybridCache) 
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
        
        await ClearUserCacheAsync(user.Id, cancellationToken);

        return Response<bool>.Success(true);
    }
    
    private async Task ClearUserCacheAsync(Guid userId, CancellationToken cancellationToken)
    {
        await hybridCache.RemoveAsync(CacheKey.User.Profile(userId), cancellationToken);
        await hybridCache.RemoveAsync(CacheKey.User.Entity(userId), cancellationToken);
    }
}


