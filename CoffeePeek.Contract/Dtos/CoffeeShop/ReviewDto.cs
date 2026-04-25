namespace CoffeePeek.Contract.Dtos.CoffeeShop;

public record ReviewDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public Guid CoffeeShopId { get; init; }
    public string Username { get; init; }

    public string Header { get; init; }
    public string Comment { get; init; }

    public RatingDto Rating { get; init; }
    public ICollection<UploadedPhotoDto> Photos { get; init; } = new List<UploadedPhotoDto>();
    public DateTime CreatedAtUtc { get; init; }
}