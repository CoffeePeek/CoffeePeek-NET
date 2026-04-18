using CoffeePeek.Client.App.ViewModels.Abstract;
using CoffeePeek.Client.App.ViewModels.Home;

namespace CoffeePeek.Client.App.ViewModels;

public sealed class MainViewModel(HeaderViewModel headerViewModel, WorkspaceViewModel workspaceViewModel, HomeViewModel homeViewModel) : ViewModelBase
{
    public HeaderViewModel HeaderViewModel { get; } = headerViewModel;
    public WorkspaceViewModel WorkspaceViewModel { get; } = workspaceViewModel;
    public HomeViewModel HomeViewModel { get; } = homeViewModel;
}
