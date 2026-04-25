namespace CoffeePeek.Client.App.Infrastructure.HTTP.Configuration;

public sealed class LocalUserSettingsOptions
{
    public string AppFolderName { get; set; } = "CoffeePeek";

    public string SettingsFileName { get; set; } = "user-settings.json";
}
