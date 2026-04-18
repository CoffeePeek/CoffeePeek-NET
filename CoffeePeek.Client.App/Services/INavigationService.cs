using CoffeePeek.Client.App.ViewModels.Abstract;

namespace CoffeePeek.Client.App.Services;

public interface INavigationService
{
    ViewModelBase CurrentView { get; }
    void NavigateTo<T>() where T : ViewModelBase;
}