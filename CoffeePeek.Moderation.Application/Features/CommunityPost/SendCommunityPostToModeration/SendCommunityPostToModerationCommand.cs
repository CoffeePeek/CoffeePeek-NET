using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CoffeePeek.Contract.Enums;
using CoffeePeek.Moderation.Domain;

namespace CoffeePeek.Moderation.Application.Features.CommunityPost.SendCommunityPostToModeration;

public record SendCommunityPostToModerationCommand(
    [property: JsonIgnore] Guid UserId,
    [property: JsonIgnore] string UserName,
    [Required] CommunityPostType PostType,
    [Required]
    [MinLength(BusinessConstants.MinCommunityPostTitleLength)]
    [MaxLength(BusinessConstants.MaxCommunityPostTitleLength)]
    string Title,
    [Required]
    [MinLength(BusinessConstants.MinCommunityPostBodyLength)]
    [MaxLength(BusinessConstants.MaxCommunityPostBodyLength)]
    string Body,
    Guid? LinkedShopId);
