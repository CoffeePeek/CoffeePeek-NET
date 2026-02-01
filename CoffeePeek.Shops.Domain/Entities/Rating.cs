using CoffeePeek.Shared.Extensions.Exceptions;

namespace CoffeePeek.Shops.Domain.Entities;

public record Rating
{
    public int Place { get; private set; }
    public int Service { get; private set; }
    public int Coffee { get; private set; }
    
    public decimal AverageRating => (Coffee + Place + Service) / 3m;

    private Rating()
    {
        //ef core
    }
    
    internal Rating(int place, int service, int coffee)
    {
        Place = place;
        Service = service;
        Coffee = coffee;
    }

    public static Rating Create(int place, int service, int coffee)
    {
        if (coffee is < BusinessConstants.MinReviewRate or > BusinessConstants.MaxReviewRate)
            throw new DomainException(
                $"{nameof(coffee)} must be between {BusinessConstants.MinReviewRate} and {BusinessConstants.MaxReviewRate}.");

        if (place is < BusinessConstants.MinReviewRate or > BusinessConstants.MaxReviewRate)
            throw new DomainException(
                $"{nameof(place)} must be between {BusinessConstants.MinReviewRate} and {BusinessConstants.MaxReviewRate}.");

        if (service is < BusinessConstants.MinReviewRate or > BusinessConstants.MaxReviewRate)
            throw new DomainException(
                $"{nameof(service)} must be between {BusinessConstants.MinReviewRate} and {BusinessConstants.MaxReviewRate}.");

        return new Rating(place, service, coffee);
    }
    
    public void Update(int coffee, int place, int service)
    {
        Coffee = coffee;
        Place = place;
        Service = service;
    }
}