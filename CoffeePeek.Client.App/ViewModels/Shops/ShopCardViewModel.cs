using System.Collections.ObjectModel;
using CoffeePeek.Client.App.ViewModels.Abstract;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CoffeePeek.Client.App.ViewModels.Shops;

public partial class ShopCardViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    [ObservableProperty]
    public partial double Rating { get; set; }

    [ObservableProperty]
    public partial bool IsTrending { get; set; }

    /// <summary>Line like «1.2 mi • Open until 8 PM».</summary>
    [ObservableProperty]
    public partial string DistanceAndHours { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Quote { get; set; } = string.Empty;

    public ObservableCollection<string> Tags { get; } = [];
}
