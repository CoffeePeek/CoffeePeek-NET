using CoffeePeek.Client.App.ViewModels.Abstract;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CoffeePeek.Client.App.Services;

public class NavigationService : ObservableObject, INavigationService
{
    private readonly Func<Type, ViewModelBase> _viewModelFactory;
    private ViewModelBase? _currentView;

    public ViewModelBase? CurrentView
    {
        get => _currentView;
        private set
        {
            if (SetProperty(ref _currentView, value))
            {
                OnPropertyChanged(nameof(HasActiveFlow));
                OnPropertyChanged(nameof(IsMainChromeVisible));
            }
        }
    }

    public bool HasActiveFlow => CurrentView is not null;

    public bool IsMainChromeVisible => CurrentView is null;

    public NavigationService(Func<Type, ViewModelBase> viewModelFactory)
    {
        _viewModelFactory = viewModelFactory;
    }

    public void NavigateTo<T>(Action<T>? configure = null) where T : ViewModelBase
    {
        var viewModel = (T)_viewModelFactory.Invoke(typeof(T));
        configure?.Invoke(viewModel);
        CurrentView = viewModel;
    }

    public void Reset() => CurrentView = null;
}