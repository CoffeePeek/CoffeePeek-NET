namespace CoffeePeek.Account.Application.Common.Interfaces;

public interface IMediaUrlProvider
{
    string? BuildAvatarUrl(string storageKey);
}
