using CoffeePeek.Moderation.Domain.Aggregates;
using CoffeePeek.Moderation.Domain.Aggregates.ModerationCommunityPostAggregate;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Exceptions;
using CoffeePeek.Shared.Kernel.Response;

namespace CoffeePeek.Moderation.Application.Features.CommunityPost.SendCommunityPostToModeration;

public static class SendCommunityPostToModerationHandler
{
    public static async Task<CreateEntityResponse> Handle(
        SendCommunityPostToModerationCommand command,
        IQueryModerationShopRepository moderationShopRepository,
        IModerationCommunityPostRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        if (command.LinkedShopId is { } linkedShopId)
        {
            var moderationShop = await moderationShopRepository.GetByPublishedShopId(linkedShopId, ct);
            if (moderationShop is null)
                throw new NotFoundException("Coffee shop not found.");
        }

        var post = ModerationCommunityPost.Create(
            command.UserId,
            command.UserName,
            CommunityPostTypeMapper.ToDomain(command.PostType),
            command.Title,
            command.Body,
            command.LinkedShopId);

        repository.Add(post);
        await unitOfWork.SaveChangesAsync(ct);

        return CreateEntityResponse.Success(entityId: post.Id);
    }
}
