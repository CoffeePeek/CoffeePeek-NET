using System.Text.Json.Serialization;

namespace CoffeePeek.MediaService.Commands;

public record GenerateAvatarUploadCommand(
    int SizeBytes,
    string FileName,
    string ContentType,
    [property: JsonIgnore] Guid OwnerId);