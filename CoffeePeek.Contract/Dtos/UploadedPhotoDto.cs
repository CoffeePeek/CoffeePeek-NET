namespace CoffeePeek.Contract.Dtos;

public record UploadedPhotoDto(
    string FileName, 
    string ContentType, 
    string StorageKey,
    long Size
);