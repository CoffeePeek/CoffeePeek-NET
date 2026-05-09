using CoffeePeek.Client.App.ViewModels.Abstract;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CoffeePeek.Client.App.ViewModels.Home;

public partial class PlaceholderPageViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial string Title { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Message { get; set; } = string.Empty;
}
