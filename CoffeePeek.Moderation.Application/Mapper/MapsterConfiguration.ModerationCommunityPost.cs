using CoffeePeek.Contract.Dtos.Public;
using CoffeePeek.Contract.Enums;
using CoffeePeek.Moderation.Application.Features.CommunityPost;
using Mapster;
using ModerationCommunityPost = CoffeePeek.Moderation.Domain.Aggregates.ModerationCommunityPostAggregate.ModerationCommunityPost;

namespace CoffeePeek.Moderation.Application.Mapper;

public partial class MapsterConfiguration
{
    private static void ConfigureModerationCommunityPost(TypeAdapterConfig config)
    {
        config.NewConfig<ModerationCommunityPost, ModerationCommunityPostDto>()
            .Map(dest => dest.PostType, src => CommunityPostTypeMapper.ToContract(src.PostType))
            .Map(dest => dest.CreatedAtUtc, src => src.CreatedAtUtc)
            .Map(dest => dest.ModerationStatus, src => (ModerationStatus)src.ModerationStatus);
    }
}
