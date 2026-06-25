using CoffeePeek.Moderation.Domain.Aggregates.ModerationCommunityPostAggregate;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Response;

namespace CoffeePeek.Moderation.Application.Features.CommunityPost.SendCommunityPostToModeration;

public static class SendCommunityPostToModerationHandler
{
    public static async Task<CreateEntityResponse> Handle(
        SendCommunityPostToModerationCommand command,
        IModerationCommunityPostRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
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
