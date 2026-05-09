using System.ComponentModel;
using Lang = CoffeePeek.Client.App.Resources.Lang.Resources;

namespace CoffeePeek.Client.App.Services;

public sealed class Loc : INotifyPropertyChanged
{
    public static readonly Loc Instance = new();

    private Loc() { }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string this[string key] =>
        Lang.ResourceManager.GetString(key, Lang.Culture) ?? key;

    internal void NotifyLanguageChanged() =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
}
