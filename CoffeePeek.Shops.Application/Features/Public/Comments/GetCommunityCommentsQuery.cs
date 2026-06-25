using CoffeePeek.Contract.Dtos.Public;
using CoffeePeek.Contract.Enums;

namespace CoffeePeek.Shops.Application.Features.Public.Comments;

public record GetCommunityCommentsQuery(
    CommunityCommentTargetType TargetType,
    Guid TargetId,
    int Page = 1,
    int PageSize = 20);

public record GetCommunityCommentsResponse(
    IReadOnlyList<CommunityCommentDto> Items,
    int TotalItems,
    int TotalPages,
    int CurrentPage,
    int PageSize);
