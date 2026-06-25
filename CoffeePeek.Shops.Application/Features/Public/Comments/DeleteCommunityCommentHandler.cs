using CoffeePeek.Shared.Domain.Interfaces.Infrastructure;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Exceptions;
using CoffeePeek.Shared.Kernel.Response;
using CoffeePeek.Shops.Application.Features.Public.Feed;
using CoffeePeek.Shops.Domain.Aggregates.CommunityCommentAggregate;

namespace CoffeePeek.Shops.Application.Features.Public.Comments;

public static class DeleteCommunityCommentHandler
{
    public static async Task<Response> Handle(
        DeleteCommunityCommentCommand command,
        ICommunityCommentRepository commentRepository,
        IQueryCommunityCommentRepository queryCommentRepository,
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        CancellationToken ct)
    {
        var comment = await commentRepository.GetById(command.CommentId, ct);
        if (comment is null || comment.IsSoftDelete)
            throw new NotFoundException("Comment not found.");

        if (comment.UserId != command.RequestingUserId && !command.CanModerate)
            throw new ForbiddenException("You do not have permission to delete this comment.");

        comment.SoftDelete();

        if (comment.ParentCommentId is null)
            await queryCommentRepository.SoftDeleteRepliesAsync(comment.Id, ct);

        await unitOfWork.SaveChangesAsync(ct);
        await CommunityFeedCacheInvalidator.InvalidateAsync(cacheService, ct);

        return Response.Success();
    }
}
