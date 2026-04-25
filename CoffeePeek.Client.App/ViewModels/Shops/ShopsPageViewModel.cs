using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using CoffeePeek.Client.App.Infrastructure.HTTP.WebClients;
using CoffeePeek.Client.App.Services;
using CoffeePeek.Client.App.ViewModels.Abstract;
using CoffeePeek.Client.App.ViewModels.Shops.Search;
using CoffeePeek.Contract.Dtos.Internal;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lang = CoffeePeek.Client.App.Resources.Lang.Resources;

namespace CoffeePeek.Client.App.ViewModels.Shops;

public partial class ShopsPageViewModel : ViewModelBase
{
    private readonly IWebCoffeeShopsClient _shopsClient;
    private readonly IWorkspaceShellNavigator _shellNavigator;
    private bool _isInitializing = true;

    private int _currentPage = 1;
    private int _totalPages = 1;
    private const int PageSize = 10;

    private CancellationTokenSource? _searchCts;
    private bool _initialLoadDone;

    public ShopsPageViewModel(IWebCoffeeShopsClient shopsClient, IWorkspaceShellNavigator shellNavigator)
    {
        _shopsClient = shopsClient;
        _shellNavigator = shellNavigator;

        try
        {
            foreach (var chip in BuildFilterChips())
                FilterChips.Add(chip);

            SelectedFilterChip = FilterChips[0];
            FilterChips[0].IsSelected = true;
        }
        finally
        {
            _isInitializing = false;
        }
    }

    [ObservableProperty]
    public partial string SearchQuery { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public bool HasMorePages => _currentPage < _totalPages;

    public ObservableCollection<FilterChipViewModel> FilterChips { get; } = [];

    [ObservableProperty]
    public partial FilterChipViewModel? SelectedFilterChip { get; set; }

    public ObservableCollection<ShopCardViewModel> Shops { get; } = [];

    [ObservableProperty]
    public partial bool IsAdditionalFiltersPanelOpen { get; set; }

    [ObservableProperty]
    public partial bool IsCityPopupOpen { get; set; }

    public ObservableCollection<CityDto> Cities { get; } = [];

    [ObservableProperty]
    public partial CityDto? SelectedCity { get; set; }

    partial void OnErrorMessageChanged(string? value) => OnPropertyChanged(nameof(HasError));

    partial void OnSearchQueryChanged(string value) => DebounceSearch();

    partial void OnSelectedCityChanged(CityDto? value)
    {
        if (_isInitializing || value is null)
            return;

        _currentPage = 1;
        LoadShopsCommand.ExecuteAsync(null);
    }

    public async Task InitializeAsync()
    {
        if (_initialLoadDone)
            return;

        _initialLoadDone = true;
        await Task.WhenAll(LoadCitiesCommand.ExecuteAsync(null), LoadShopsCommand.ExecuteAsync(null));
    }

    [RelayCommand]
    private async Task LoadShopsAsync()
    {
        ErrorMessage = null;
        IsLoading = true;

        try
        {
            var result = await _shopsClient.SearchAsync(
                string.IsNullOrWhiteSpace(SearchQuery) ? null : SearchQuery.Trim(),
                SelectedCity?.Id,
                _currentPage,
                PageSize);

            if (result.IsFailed)
            {
                ErrorMessage = result.Errors.FirstOrDefault()?.Message ?? Lang.ShopPage_LoadError;
                return;
            }

            var data = result.Value;
            _totalPages = data.TotalPages;

            Shops.Clear();
            foreach (var dto in data.CoffeeShops)
                Shops.Add(ShopCardViewModel.FromDto(dto));

            OnPropertyChanged(nameof(HasMorePages));
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task LoadNextPageAsync()
    {
        if (!HasMorePages || IsLoading)
            return;

        _currentPage++;
        ErrorMessage = null;
        IsLoading = true;

        try
        {
            var result = await _shopsClient.SearchAsync(
                string.IsNullOrWhiteSpace(SearchQuery) ? null : SearchQuery.Trim(),
                SelectedCity?.Id,
                _currentPage,
                PageSize);

            if (result.IsFailed)
            {
                _currentPage--;
                ErrorMessage = result.Errors.FirstOrDefault()?.Message ?? Lang.ShopPage_LoadError;
                return;
            }

            var data = result.Value;
            _totalPages = data.TotalPages;

            foreach (var dto in data.CoffeeShops)
                Shops.Add(ShopCardViewModel.FromDto(dto));

            OnPropertyChanged(nameof(HasMorePages));
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task LoadCitiesAsync()
    {
        var result = await _shopsClient.GetCitiesAsync();
        if (result.IsFailed)
            return;

        Cities.Clear();
        foreach (var city in result.Value.Cities)
            Cities.Add(city);

        if (Cities.Count > 0)
        {
            SelectedCity = Cities[0];
            var cityChip = FilterChips.FirstOrDefault(c => c.FilterType == FilterType.City);
            if (cityChip is not null)
                cityChip.DisplayName = Cities[0].Name;
        }
    }

    [RelayCommand]
    private void OpenShop(Guid shopId) => _shellNavigator.OpenShopDetail(shopId);

    [RelayCommand]
    private void ToggleAdditionalFilters() => IsAdditionalFiltersPanelOpen = !IsAdditionalFiltersPanelOpen;

    [RelayCommand]
    private void SelectCity(CityDto city)
    {
        SelectedCity = city;
        var cityChip = FilterChips.First(c => c.FilterType == FilterType.City);
        cityChip.DisplayName = city.Name;
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

    private void DebounceSearch()
    {
        _searchCts?.Cancel();
        _searchCts = new CancellationTokenSource();
        var token = _searchCts.Token;

        _ = Task.Delay(350, token).ContinueWith(_ =>
        {
            _currentPage = 1;
            LoadShopsCommand.ExecuteAsync(null);
        }, token, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default);
    }

    private static IEnumerable<FilterChipViewModel> BuildFilterChips() =>
    [
        new(Lang.ShopPage_Filter_City, FilterType.City),
        new(Lang.ShopPage_Filter_OpenTime, FilterType.OpenTime),
        new(Lang.ShopPage_Filter_New, FilterType.New),
        new(Lang.ShopPage_Filter_Favorite, FilterType.Favorite),
        new(Lang.ShopPage_Filter_Visited, FilterType.Visited)
    ];
}
