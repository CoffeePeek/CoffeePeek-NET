using CoffeePeek.Client.App.ViewModels.Abstract;

namespace CoffeePeek.Client.App.Services;

public interface INavigationService
{
    ViewModelBase? CurrentView { get; }

    bool HasActiveFlow { get; }

    /// <summary>When false, only the auth/welcome flow is shown (no header or workspace).</summary>
    bool IsMainChromeVisible { get; }

    void NavigateTo<T>(Action<T>? configure = null) where T : ViewModelBase;

    void Reset();
}