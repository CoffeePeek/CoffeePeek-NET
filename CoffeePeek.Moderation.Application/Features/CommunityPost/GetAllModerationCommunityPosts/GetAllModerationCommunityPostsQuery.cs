using CoffeePeek.Contract.Dtos.Public;
using CoffeePeek.Contract.Enums;

namespace CoffeePeek.Moderation.Application.Features.CommunityPost.GetAllModerationCommunityPosts;

public record GetAllModerationCommunityPostsQuery(
    int Page = 1,
    int PageSize = 20,
    ModerationStatus? Status = null,
    string? Search = null);

public record GetAllModerationCommunityPostsResponse(
    ModerationCommunityPostDto[] Items,
    int TotalItems,
    int TotalPages,
    int CurrentPage,
    int PageSize);
