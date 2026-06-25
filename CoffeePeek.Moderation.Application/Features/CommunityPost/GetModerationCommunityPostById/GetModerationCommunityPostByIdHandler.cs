using CoffeePeek.Contract.Dtos.Public;
using CoffeePeek.Moderation.Domain.Aggregates.ModerationCommunityPostAggregate;
using CoffeePeek.Shared.Kernel.Response;
using MapsterMapper;

namespace CoffeePeek.Moderation.Application.Features.CommunityPost.GetModerationCommunityPostById;

public static class GetModerationCommunityPostByIdHandler
{
    public static async Task<Response<ModerationCommunityPostDto>> Handle(
        GetModerationCommunityPostByIdQuery query,
        IQueryModerationCommunityPostRepository repository,
        IMapper mapper,
        CancellationToken ct)
    {
        var post = await repository.GetById(query.PostId, ct);

        if (post is null)
            return Response<ModerationCommunityPostDto>.Error("Moderation community post not found.");

        return Response<ModerationCommunityPostDto>.Success(mapper.Map<ModerationCommunityPostDto>(post));
    }
}
