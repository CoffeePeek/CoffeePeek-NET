using CoffeePeek.Account.Domain.Entities;
using CoffeePeek.Account.Domain.Entities.PhotoMetadataAggregate;
using CoffeePeek.Account.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Account.Persistence.Repositories;

public class PhotoMetadataRepository(AccountDbContext dbContext) : IPhotoMetadataRepository
{
    private readonly DbSet<PhotoMetadata> _repository = dbContext.Photos;
    public void Add(PhotoMetadata photoMetadata)
    {
        _repository.Add(photoMetadata);
    }
}