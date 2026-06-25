using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CoffeePeek.Contract.Enums;
using CoffeePeek.Shops.Domain;

namespace CoffeePeek.Shops.Application.Features.Public.Comments;

public record CreateCommunityCommentCommand(
    [Required] CommunityCommentTargetType TargetType,
    [Required] Guid TargetId,
    Guid? ParentCommentId,
    [Required]
    [MinLength(BusinessConstants.MinCommunityCommentBodyLength)]
    [MaxLength(BusinessConstants.MaxCommunityCommentBodyLength)]
    string Body)
{
    [JsonIgnore] public Guid UserId { get; init; }
    [JsonIgnore] public string UserName { get; init; } = string.Empty;
}
