using System.Collections.ObjectModel;
using System.Linq;
using CoffeePeek.Client.App.ViewModels.Abstract;
using CoffeePeek.Client.App.ViewModels.Shops.Search;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lang = CoffeePeek.Client.App.Resources.Lang.Resources;

namespace CoffeePeek.Client.App.ViewModels.Shops;

public partial class ShopsPageViewModel : ViewModelBase
{
    private bool _isInitializing = true;

    public ShopsPageViewModel()
    {
        try
        {
            foreach (var chip in BuildFilterChips())
                FilterChips.Add(chip);

            SelectedFilterChip = FilterChips[0];
            FilterChips[0].IsSelected = true;

            foreach (var shop in BuildMockShops())
                Shops.Add(shop);

            foreach (var city in MockCities)
                Cities.Add(city);

            SelectedCity = Cities[0];
        }
        finally
        {
            _isInitializing = false;
        }
    }

    [ObservableProperty]
    public partial string SearchQuery { get; set; } = string.Empty;

    public ObservableCollection<FilterChipViewModel> FilterChips { get; } = [];

    [ObservableProperty]
    public partial FilterChipViewModel? SelectedFilterChip { get; set; }

    public ObservableCollection<ShopCardViewModel> Shops { get; } = [];

    [ObservableProperty]
    public partial bool IsAdditionalFiltersPanelOpen { get; set; }

    [ObservableProperty]
    public partial bool IsCityPopupOpen { get; set; }

    public ObservableCollection<string> Cities { get; } = [];

    [ObservableProperty]
    public partial string? SelectedCity { get; set; }

    private static IEnumerable<string> MockCities => ["Minsk", "Gomel", "Brest", "Grodno", "Mogilev"];

    [RelayCommand]
    private void ToggleAdditionalFilters() => IsAdditionalFiltersPanelOpen = !IsAdditionalFiltersPanelOpen;

    [RelayCommand]
    private void SelectCity(string city)
    {
        SelectedCity = city;
        var cityChip = FilterChips.First(c => c.FilterType == FilterType.City);
        cityChip.DisplayName = city;
        IsCityPopupOpen = false;
    }

    partial void OnSelectedFilterChipChanged(FilterChipViewModel? value)
    {
        if (value is null)
            return;

        foreach (var c in FilterChips)
            c.IsSelected = ReferenceEquals(c, value);

        if (_isInitializing)
            return;

        if (value.FilterType == FilterType.City)
            IsCityPopupOpen = true;
        else
            IsCityPopupOpen = false;
    }

    private static IEnumerable<FilterChipViewModel> BuildFilterChips() =>
    [
        new(Lang.ShopPage_Filter_City, FilterType.City),
        new(Lang.ShopPage_Filter_OpenTime, FilterType.OpenTime),
        new(Lang.ShopPage_Filter_New, FilterType.New),
        new(Lang.ShopPage_Filter_Favorite, FilterType.Favorite),
        new(Lang.ShopPage_Filter_Visited, FilterType.Visited)
    ];

    private static IEnumerable<ShopCardViewModel> BuildMockShops()
    {
        var a = new ShopCardViewModel
        {
            Name = "Bean There",
            Rating = 4.9,
            IsTrending = true,
            DistanceAndHours = "1.2 mi • Open until 8 PM",
            Quote = "Best for quiet work!"
        };
        foreach (var t in new[] { "ARTISAN", "DOWNTOWN", "QUIET" })
            a.Tags.Add(t);

        var b = new ShopCardViewModel
        {
            Name = "The Roasted Bean",
            Rating = 4.8,
            IsTrending = false,
            DistanceAndHours = "0.5 mi • Open until 8 PM",
            Quote = "Best for lattes!"
        };
        foreach (var t in new[] { "FREE WIFI", "VEGAN", "QUIET" })
            b.Tags.Add(t);

        var c = new ShopCardViewModel
        {
            Name = "Espresso Lab",
            Rating = 4.7,
            IsTrending = true,
            DistanceAndHours = "0.8 mi • Closes soon",
            Quote = "Modern and chic ambiance!"
        };
        foreach (var t in new[] { "MODERN", "UPTOWN", "HIRING" })
            c.Tags.Add(t);

        return [a, b, c];
    }
}
