namespace CoffeePeek.Contract.Dtos;

public class ShortPhotoMetadataDto
{
    public string FileName { get; private set; }
    public string StorageKey { get; private set; }
    public string FullUrl { get; private set; }
}