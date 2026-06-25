using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Exceptions;
using CoffeePeek.Shared.Kernel.Response;
using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;
using CoffeePeek.Shops.Domain.Aggregates.CommunityFollowAggregate;

namespace CoffeePeek.Shops.Application.Features.Public.Follows;

public static class FollowCityHandler
{
    public static async Task<Response> Handle(
        FollowCityCommand command,
        ICommunityCityFollowRepository followRepository,
        IQueryCityRepository cityRepository,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        if (!await cityRepository.Exists(command.CityId, ct))
            throw new NotFoundException("City not found.");

        if (await followRepository.GetAsync(command.UserId, command.CityId, ct) is not null)
            return Response.Success("Already following this city.");

        followRepository.Add(CommunityCityFollow.Create(command.UserId, command.CityId));
        await unitOfWork.SaveChangesAsync(ct);

        return Response.Success("City followed.");
    }
}

public static class UnfollowCityHandler
{
    public static async Task<Response> Handle(
        UnfollowCityCommand command,
        ICommunityCityFollowRepository followRepository,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        var follow = await followRepository.GetAsync(command.UserId, command.CityId, ct);
        if (follow is null)
            throw new NotFoundException("City follow not found.");

        followRepository.Remove(follow);
        await unitOfWork.SaveChangesAsync(ct);

        return Response.Success("City unfollowed.");
    }
}

public static class GetFollowedCitiesHandler
{
    public static async Task<Response<GetFollowedCitiesResponse>> Handle(
        GetFollowedCitiesQuery query,
        IQueryCommunityCityFollowRepository followRepository,
        CancellationToken ct)
    {
        var cityIds = await followRepository.GetFollowedCityIdsAsync(query.UserId, ct);
        return Response<GetFollowedCitiesResponse>.Success(new GetFollowedCitiesResponse(cityIds));
    }
}
