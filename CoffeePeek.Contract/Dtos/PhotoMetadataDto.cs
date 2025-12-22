namespace CoffeePeek.Contract.Dtos;

public class PhotoMetadataDto
{
    public string FileName { get; set; }
    public string ContentType { get; set; }
    public byte[] Data { get; set; }
}