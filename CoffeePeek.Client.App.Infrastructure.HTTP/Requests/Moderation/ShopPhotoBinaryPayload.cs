namespace CoffeePeek.Client.App.Infrastructure.HTTP.Requests.Moderation;

public sealed record ShopPhotoBinaryPayload(
    string FileName,
    string ContentType,
    byte[] Content);
