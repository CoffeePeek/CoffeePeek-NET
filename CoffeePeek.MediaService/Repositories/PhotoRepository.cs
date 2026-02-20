using CoffeePeek.MediaService.Data;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.MediaService.Repositories;

public class PhotoRepository(MediaDbContext dbContext) : IPhotoRepository
{
    private readonly DbSet<PhotoMetadata> _repository = dbContext.Photos;
    
    public void Add(PhotoMetadata metadata)
    {
        _repository.Add(metadata);
    }

    public void AddRange(IEnumerable<PhotoMetadata> photoMetadatas)
    {
        _repository.AddRange(photoMetadatas);
    }
    
    public Task<PhotoMetadata?> GetByIdAsync(Guid photoId, CancellationToken ct)
    {
        return _repository.FirstOrDefaultAsync(x => x.Id == photoId, ct);
    }
}