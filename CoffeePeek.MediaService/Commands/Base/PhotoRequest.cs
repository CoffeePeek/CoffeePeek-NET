namespace CoffeePeek.MediaService.Commands.Base;

public record PhotoRequest(
    int SizeBytes,
    string FileName,
    string ContentType);