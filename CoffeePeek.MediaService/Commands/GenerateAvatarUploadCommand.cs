using System.Text.Json.Serialization;
using CoffeePeek.MediaService.Commands.Base;

namespace CoffeePeek.MediaService.Commands;

public record GenerateAvatarUploadCommand(
    long SizeBytes,
    string FileName,
    string ContentType,
    [property: JsonIgnore] Guid OwnerId)
    : PhotoRequest(SizeBytes, FileName, ContentType);