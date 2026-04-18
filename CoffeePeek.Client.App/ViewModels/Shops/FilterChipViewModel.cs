using CoffeePeek.Client.App.ViewModels.Abstract;
using CoffeePeek.Client.App.ViewModels.Shops.Search;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CoffeePeek.Client.App.ViewModels.Shops;

public partial class FilterChipViewModel : ViewModelBase
{
    public FilterChipViewModel(string displayName, FilterType filterType)
    {
        DisplayName = displayName;
        FilterType = filterType;
    }

    [ObservableProperty]
    public partial string DisplayName { get; set; }

    public FilterType FilterType { get; }

    [ObservableProperty]
    public partial bool IsSelected { get; set; }
}
