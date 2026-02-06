using System.Text.Json.Serialization;

namespace CoffeePeek.MediaService.Requests;

public record UploadUrlRequest( 
    string FileName,
    string ContentType,
    long SizeBytes,
    [property:JsonIgnore]Guid OwnerId);