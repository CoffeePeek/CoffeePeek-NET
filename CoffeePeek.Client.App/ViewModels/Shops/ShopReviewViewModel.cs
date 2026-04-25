using System.Globalization;
using CoffeePeek.Contract.Dtos.CoffeeShop;

namespace CoffeePeek.Client.App.ViewModels.Shops;

public sealed class ShopReviewViewModel
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public bool IsOwnReview { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Header { get; init; } = string.Empty;
    public string Comment { get; init; } = string.Empty;
    public bool HasComment { get; init; }
    public string StarsText { get; init; } = string.Empty;
    public string DateLabel { get; init; } = string.Empty;

    public static ShopReviewViewModel From(ReviewDto dto, Guid? currentUserId)
    {
        var avg = (dto.Rating.Place + dto.Rating.Service + dto.Rating.Coffee) / 3.0;
        var stars = new string('★', (int)Math.Round(avg)) + new string('☆', 5 - (int)Math.Round(avg));

        return new ShopReviewViewModel
        {
            Id = dto.Id,
            UserId = dto.UserId,
            IsOwnReview = currentUserId == dto.UserId,
            Username = dto.Username,
            Header = dto.Header,
            Comment = dto.Comment,
            HasComment = !string.IsNullOrWhiteSpace(dto.Comment),
            StarsText = stars,
            DateLabel = dto.CreatedAtUtc.ToString("dd MMM yyyy", CultureInfo.InvariantCulture)
        };
    }
}
