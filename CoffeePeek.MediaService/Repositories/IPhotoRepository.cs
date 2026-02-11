using CoffeePeek.MediaService.Data;

namespace CoffeePeek.MediaService.Repositories;

public interface IPhotoRepository
{
    void Add(PhotoMetadata metadata);
    Task<PhotoMetadata?> GetByIdAsync(Guid commandPhotoId, CancellationToken ct);
}