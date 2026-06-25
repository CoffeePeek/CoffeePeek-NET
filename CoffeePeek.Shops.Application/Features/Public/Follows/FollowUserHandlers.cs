using CoffeePeek.Contract.Events.Community;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Exceptions;
using CoffeePeek.Shared.Kernel.Response;
using CoffeePeek.Shops.Application.Abstractions;
using CoffeePeek.Shops.Domain.Aggregates.CommunityFollowAggregate;

namespace CoffeePeek.Shops.Application.Features.Public.Follows;

public static class FollowUserHandler
{
    public static async Task<(Response, CommunityFollowNotificationEvent?)> Handle(
        FollowUserCommand command,
        ICommunityUserFollowRepository followRepository,
        IUserExistenceLookup userExistenceLookup,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        if (!await userExistenceLookup.ExistsAsync(command.FollowingUserId, ct))
            throw new NotFoundException("User not found.");

        if (await followRepository.GetAsync(command.FollowerId, command.FollowingUserId, ct) is not null)
            return (Response.Success("Already following this user."), null);

        var follow = CommunityUserFollow.Create(command.FollowerId, command.FollowingUserId);
        followRepository.Add(follow);

        try
        {
            await unitOfWork.SaveChangesAsync(ct);
        }
        catch (Exception)
        {
            if (await followRepository.GetAsync(command.FollowerId, command.FollowingUserId, ct) is not null)
                return (Response.Success("Already following this user."), null);

            throw;
        }

        var notificationEvent = new CommunityFollowNotificationEvent(
            command.FollowingUserId,
            command.FollowerId,
            command.FollowerUserName);

        return (Response.Success("User followed."), notificationEvent);
    }
}

public static class UnfollowUserHandler
{
    public static async Task<Response> Handle(
        UnfollowUserCommand command,
        ICommunityUserFollowRepository followRepository,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        var follow = await followRepository.GetAsync(command.FollowerId, command.FollowingUserId, ct);
        if (follow is null)
            throw new NotFoundException("Follow relationship not found.");

        followRepository.Remove(follow);
        await unitOfWork.SaveChangesAsync(ct);

        return Response.Success("User unfollowed.");
    }
}

public static class GetFollowingHandler
{
    public static async Task<Response<GetFollowingResponse>> Handle(
        GetFollowingQuery query,
        IQueryCommunityUserFollowRepository followRepository,
        CancellationToken ct)
    {
        var followingUserIds = await followRepository.GetFollowingUserIdsAsync(query.UserId, ct);
        return Response<GetFollowingResponse>.Success(new GetFollowingResponse(followingUserIds));
    }
}
