namespace CoffeePeek.MediaService.Commands;

public record GenerateAvatarUploadCommand
{
    public int SizeBytes { get; init; }
    public string FileName { get; set; }
    public string ContentType { get; set; }
    public Guid OwnerId { get; set; }
}