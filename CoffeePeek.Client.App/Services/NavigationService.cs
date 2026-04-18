using CoffeePeek.Client.App.ViewModels.Abstract;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CoffeePeek.Client.App.Services;

public class NavigationService : ObservableObject, INavigationService
{
    private readonly Func<Type, ViewModelBase> _viewModelFactory;
    private ViewModelBase _currentView;

    public ViewModelBase CurrentView
    {
        get => _currentView;
        private set => SetProperty(ref _currentView, value);
    }

    public NavigationService(Func<Type, ViewModelBase> viewModelFactory)
    {
        _viewModelFactory = viewModelFactory;
    }

    public void NavigateTo<T>() where T : ViewModelBase
    {
        var viewModel = _viewModelFactory.Invoke(typeof(T));
        CurrentView = viewModel;
    }
}