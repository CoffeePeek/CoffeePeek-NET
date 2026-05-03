namespace CoffeePeek.MediaService.Commands.Base;

public record PhotoRequest(
    long SizeBytes,
    string FileName,
    string ContentType);