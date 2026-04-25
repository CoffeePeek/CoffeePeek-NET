using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using CoffeePeek.Client.App.Infrastructure.HTTP.WebClients;
using CoffeePeek.Client.App.Services;
using CoffeePeek.Client.App.ViewModels.Abstract;
using CoffeePeek.Client.App.ViewModels.Shops.Search;
using CoffeePeek.Contract.Dtos.Internal;
using CoffeePeek.Contract.Dtos.Shop;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lang = CoffeePeek.Client.App.Resources.Lang.Resources;

namespace CoffeePeek.Client.App.ViewModels.Shops;

public partial class ShopsPageViewModel : ViewModelBase
{
    private readonly IWebCoffeeShopsClient _shopsClient;
    private readonly IWebCatalogsClient _catalogsClient;
    private readonly IWorkspaceShellNavigator _shellNavigator;
    private readonly CatalogFilterGroupViewModel _beanFilters = new(Lang.ShopPage_FilterBeans);
    private readonly CatalogFilterGroupViewModel _roasterFilters = new(Lang.ShopPage_FilterRoasters);
    private readonly CatalogFilterGroupViewModel _brewMethodFilters = new(Lang.ShopPage_FilterBrewMethods);
    private readonly CatalogFilterGroupViewModel _equipmentFilters = new(Lang.ShopPage_FilterEquipment);
    private bool _isInitializing = true;

    private int _currentPage = 1;
    private int _totalPages = 1;
    private const int PageSize = 10;

    private CancellationTokenSource? _searchCts;
    private bool _initialLoadDone;

    public ShopsPageViewModel(
        IWebCoffeeShopsClient shopsClient,
        IWebCatalogsClient catalogsClient,
        IWorkspaceShellNavigator shellNavigator)
    {
        _shopsClient = shopsClient;
        _catalogsClient = catalogsClient;
        _shellNavigator = shellNavigator;

        foreach (var group in new[] { _beanFilters, _roasterFilters, _brewMethodFilters, _equipmentFilters })
        {
            group.PropertyChanged += OnFilterGroupPropertyChanged;
            FilterGroups.Add(group);
        }

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

    [ObservableProperty]
    public partial bool IsLoadingFilters { get; set; }

    public ObservableCollection<CityDto> Cities { get; } = [];

    [ObservableProperty]
    public partial CityDto? SelectedCity { get; set; }

    public ObservableCollection<CatalogFilterGroupViewModel> FilterGroups { get; } = [];

    public int ActiveFilterCount => FilterGroups.Sum(g => g.ActiveFilterCount);

    partial void OnErrorMessageChanged(string? value) => OnPropertyChanged(nameof(HasError));

    partial void OnSearchQueryChanged(string value) => DebounceSearch();

    async partial void OnSelectedCityChanged(CityDto? value)
    {
        if (_isInitializing || value is null)
            return;

        _currentPage = 1;
        await LoadShopsAsync();
    }

    public async Task InitializeAsync()
    {
        if (_initialLoadDone)
            return;

        _initialLoadDone = true;
        await Task.WhenAll(
            LoadCitiesCommand.ExecuteAsync(null),
            LoadShopsCommand.ExecuteAsync(null),
            LoadCatalogFiltersCommand.ExecuteAsync(null));
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
                _roasterFilters.SelectedIds,
                _beanFilters.SelectedIds,
                _brewMethodFilters.SelectedIds,
                _equipmentFilters.SelectedIds,
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
                _roasterFilters.SelectedIds,
                _beanFilters.SelectedIds,
                _brewMethodFilters.SelectedIds,
                _equipmentFilters.SelectedIds,
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
    private async Task LoadCitiesAsync(CancellationToken ct = default)
    {
        var result = await _catalogsClient.GetCitiesAsync(ct);
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
    private async Task LoadCatalogFiltersAsync(CancellationToken ct = default)
    {
        IsLoadingFilters = true;

        try
        {
            var beansTask = _catalogsClient.GetBeansAsync(ct);
            var roastersTask = _catalogsClient.GetRoastersAsync(ct);
            var equipmentTask = _catalogsClient.GetEquipmentAsync(ct);
            var brewMethodsTask = _catalogsClient.GetBrewMethodsAsync(ct);

            await Task.WhenAll(beansTask, roastersTask, equipmentTask, brewMethodsTask);

            if (beansTask.Result.IsSuccess)
                ReplaceFilterItems(_beanFilters, beansTask.Result.Value.Beans, static b => new CatalogFilterItemViewModel { Id = b.Id, Name = b.Name });

            if (roastersTask.Result.IsSuccess)
                ReplaceFilterItems(_roasterFilters, roastersTask.Result.Value.Roasters, static r => new CatalogFilterItemViewModel { Id = r.Id, Name = r.Name });

            if (equipmentTask.Result.IsSuccess)
                ReplaceFilterItems(_equipmentFilters, equipmentTask.Result.Value.Equipment, static e => new CatalogFilterItemViewModel { Id = e.Id, Name = e.Name });

            if (brewMethodsTask.Result.IsSuccess)
                ReplaceFilterItems(_brewMethodFilters, brewMethodsTask.Result.Value.BrewMethods, static m => new CatalogFilterItemViewModel { Id = m.Id, Name = m.Name });
        }
        finally
        {
            IsLoadingFilters = false;
        }
    }

    [RelayCommand]
    private async Task ApplyFiltersAsync()
    {
        _currentPage = 1;
        OnPropertyChanged(nameof(ActiveFilterCount));
        await LoadShopsAsync();
    }

    [RelayCommand]
    private async Task ClearFiltersAsync()
    {
        foreach (var group in FilterGroups)
            group.ClearSelection();

        _currentPage = 1;
        OnPropertyChanged(nameof(ActiveFilterCount));
        await LoadShopsAsync();
    }

    [RelayCommand]
    private void OpenShop(Guid shopId) => _shellNavigator.OpenShopDetail(shopId);

    [RelayCommand]
    private void OpenSuggestShop() => _shellNavigator.OpenSuggestShop();

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
        _ = DebounceSearchAsync(_searchCts.Token);
    }

    private async Task DebounceSearchAsync(CancellationToken token)
    {
        try
        {
            await Task.Delay(350, token);
            _currentPage = 1;
            await LoadShopsAsync();
        }
        catch (OperationCanceledException)
        {
            // A newer search query superseded this one.
        }
    }

    internal static Guid[]? GetSelectedIds(IEnumerable<CatalogFilterItemViewModel> filters) =>
        CatalogFilterGroupViewModel.GetSelectedIds(filters);

    private static void ReplaceFilterItems<T>(
        CatalogFilterGroupViewModel group,
        IEnumerable<T> items,
        Func<T, CatalogFilterItemViewModel> map)
    {
        group.ReplaceItems(items.Select(map));
    }

    private void OnFilterGroupPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CatalogFilterGroupViewModel.ActiveFilterCount) ||
            e.PropertyName == nameof(CatalogFilterGroupViewModel.SelectedIds))
            OnPropertyChanged(nameof(ActiveFilterCount));
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
