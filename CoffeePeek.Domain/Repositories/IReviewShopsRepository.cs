namespace CoffeePeek.Domain.Repositories;

public interface IReviewShopsRepository
{
    Task<bool> UpdatePhotos(int shopId, int userId, ICollection<string> urls);
}