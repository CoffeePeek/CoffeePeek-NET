namespace CoffeePeek.Client.App.Startup;

public static class BeforeMainShellOrders
{
    public const int ApplyPersistedLanguage = -1000; // before theme — views read resources on first creation
    public const int ApplyPersistedTheme = -900;
}
