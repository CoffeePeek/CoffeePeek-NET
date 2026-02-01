using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Dtos;
using CoffeePeek.Moderation.Domain;
using MediatR;

namespace CoffeePeek.Moderation.Application.Features.Review.SendReviewToModeration;

/// <summary>
/// 
/// </summary>
/// <param name="UserId"></param>
/// <param name="UserName"></param>
/// <param name="ShopId"></param>
/// <param name="Header"></param>
/// <param name="Comment"></param>
/// <param name="Rating"></param>
/// <param name="Photos">Already loaded photos to S3</param>
/// <param name="CheckInId">Property for creating review from checkIn</param>
public record SendReviewToModerationCommand(
    [property: JsonIgnore] Guid UserId,
    [property: JsonIgnore] string UserName,
    Guid ShopId,
    [MaxLength(BusinessConstants.MaxReviewHeaderLength)]
    string Header,
    [MaxLength(BusinessConstants.MaxReviewCommentLength)]
    string Comment,
    RatingDto Rating,
    ICollection<UploadedPhotoDto>? Photos,
    [property: JsonIgnore] Guid? CheckInId = null) : IRequest<CreateEntityResponse>;