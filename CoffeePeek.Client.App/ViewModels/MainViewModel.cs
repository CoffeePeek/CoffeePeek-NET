using CoffeePeek.Client.App.Services;
using CoffeePeek.Client.App.ViewModels.Abstract;
using CoffeePeek.Client.App.ViewModels.Home;

namespace CoffeePeek.Client.App.ViewModels;

public sealed class MainViewModel(
    HeaderViewModel headerViewModel,
    WorkspaceViewModel workspaceViewModel,
    HomeViewModel homeViewModel,
    INavigationService navigation,
    ILayoutBreakpointService layout) : ViewModelBase
{
    public HeaderViewModel HeaderViewModel { get; } = headerViewModel;
    public WorkspaceViewModel WorkspaceViewModel { get; } = workspaceViewModel;
    public HomeViewModel HomeViewModel { get; } = homeViewModel;

    public INavigationService Navigation { get; } = navigation;
    public ILayoutBreakpointService Layout { get; } = layout;
}
