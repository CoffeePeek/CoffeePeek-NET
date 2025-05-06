using CoffeePeek.Domain.Enums;

namespace CoffeePeek.Domain.Entities.Review;

public class RatingCategory : BaseEntity
{
    public RatingCategoryType Type { get; set; }
}