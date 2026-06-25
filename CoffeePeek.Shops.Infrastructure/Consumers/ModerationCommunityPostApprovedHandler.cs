using CoffeePeek.Contract.Dtos.Public;
using CoffeePeek.Contract.Events.Moderation;
using CoffeePeek.Shared.Domain.Interfaces.Infrastructure;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shops.Application.Features.Public.Feed;
using CoffeePeek.Shops.Application.Features.Public.Posts;
using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;
using CoffeePeek.Shops.Domain.Aggregates.CommunityPostAggregate;

namespace CoffeePeek.Shops.Infrastructure.Consumers;

public static class ModerationCommunityPostApprovedHandler
{
    public static async Task Handle(
        ModerationCommunityPostApprovedEvent @event,
        ICommunityPostRepository postRepository,
        IQueryCoffeeShopRepository coffeeShopRepository,
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        CancellationToken ct)
    {
        var dto = @event.Post;

        if (await postRepository.ExistsByModerationPostIdAsync(dto.Id, ct))
            return;

        if (dto.LinkedShopId is { } linkedShopId && !await coffeeShopRepository.Exists(linkedShopId, ct))
            return;

        var post = CommunityPost.Create(
            dto.UserId,
            dto.UserName,
            CommunityPostTypeMapper.ToDomain(dto.PostType),
            dto.Title,
            dto.Body,
            dto.LinkedShopId,
            dto.Id);

        postRepository.Add(post);
        await unitOfWork.SaveChangesAsync(ct);
        await CommunityFeedCacheInvalidator.InvalidateAsync(cacheService, ct);
    }
}
