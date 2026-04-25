using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using CoffeePeek.Client.App.Infrastructure.HTTP.WebClients;
using CoffeePeek.Client.App.Services;
using CoffeePeek.Client.App.ViewModels.Abstract;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lang = CoffeePeek.Client.App.Resources.Lang.Resources;

namespace CoffeePeek.Client.App.ViewModels.Shops;

public partial class ShopDetailViewModel : ViewModelBase
{
    private readonly IWebCoffeeShopsClient _shopsClient;
    private readonly IWorkspaceShellNavigator _shellNavigator;

    public ShopDetailViewModel(
        IWebCoffeeShopsClient shopsClient,
        IWorkspaceShellNavigator shellNavigator)
    {
        _shopsClient = shopsClient;
        _shellNavigator = shellNavigator;
    }

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? Description { get; set; }

    [ObservableProperty]
    public partial double Rating { get; set; }

    [ObservableProperty]
    public partial string RatingLabel { get; set; } = string.Empty;

    [ObservableProperty]
    public partial int ReviewCount { get; set; }

    [ObservableProperty]
    public partial bool IsOpen { get; set; }

    [ObservableProperty]
    public partial bool IsNew { get; set; }

    [ObservableProperty]
    public partial bool IsFavorite { get; set; }

    [ObservableProperty]
    public partial string PriceLabel { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? Address { get; set; }

    [ObservableProperty]
    public partial bool HasAddress { get; set; }

    [ObservableProperty]
    public partial bool HasDescription { get; set; }

    [ObservableProperty]
    public partial bool HasSchedule { get; set; }

    [ObservableProperty]
    public partial bool HasContact { get; set; }

    [ObservableProperty]
    public partial bool HasReviews { get; set; }

    [ObservableProperty]
    public partial string? Phone { get; set; }

    [ObservableProperty]
    public partial string? Email { get; set; }

    [ObservableProperty]
    public partial string? Website { get; set; }

    [ObservableProperty]
    public partial string? Instagram { get; set; }

    public ObservableCollection<string> Tags { get; } = [];
    public ObservableCollection<ScheduleLineViewModel> Schedule { get; } = [];
    public ObservableCollection<ShopReviewViewModel> Reviews { get; } = [];

    partial void OnErrorMessageChanged(string? value) => OnPropertyChanged(nameof(HasError));

    public async Task LoadAsync(Guid shopId)
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var result = await _shopsClient.GetByIdAsync(shopId);

            if (result.IsFailed)
            {
                ErrorMessage = result.Errors.FirstOrDefault()?.Message ?? Lang.ShopDetail_LoadError;
                return;
            }

            var shop = result.Value.ShopDto;
            MapFromDto(shop);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void GoBack() => _shellNavigator.CloseShopDetail();

    private void MapFromDto(CoffeeShopDetailsDto dto)
    {
        Name = dto.Name;
        Description = dto.Description;
        HasDescription = !string.IsNullOrWhiteSpace(dto.Description);
        Rating = (double)dto.Rating;
        RatingLabel = dto.Rating.ToString("0.0", CultureInfo.InvariantCulture);
        ReviewCount = dto.ReviewCount;
        IsOpen = dto.IsOpen;
        IsNew = dto.IsNew;
        IsFavorite = dto.IsFavorite;
        PriceLabel = dto.PriceRange switch
        {
            PriceRange.Cheap => "$",
            PriceRange.Moderate => "$$",
            PriceRange.Expensive => "$$$",
            PriceRange.Luxury => "$$$$",
            _ => ""
        };

        if (dto.Location is not null)
        {
            Address = dto.Location.Address;
            HasAddress = !string.IsNullOrWhiteSpace(dto.Location.Address);
        }

        Tags.Clear();
        if (dto.Roasters is not null)
            foreach (var r in dto.Roasters) Tags.Add(r.Name);
        if (dto.CoffeeBeans is not null)
            foreach (var b in dto.CoffeeBeans) Tags.Add(b.Name);
        if (dto.BrewMethods is not null)
            foreach (var m in dto.BrewMethods) Tags.Add(m.Name);
        if (dto.Equipments is not null)
            foreach (var e in dto.Equipments) Tags.Add(e.Name);

        HasContact = dto.ShopContact is not null &&
                     (!string.IsNullOrWhiteSpace(dto.ShopContact.PhoneNumber) ||
                      !string.IsNullOrWhiteSpace(dto.ShopContact.Email) ||
                      !string.IsNullOrWhiteSpace(dto.ShopContact.SiteLink) ||
                      !string.IsNullOrWhiteSpace(dto.ShopContact.InstagramLink));

        if (dto.ShopContact is not null)
        {
            Phone = dto.ShopContact.PhoneNumber;
            Email = dto.ShopContact.Email;
            Website = dto.ShopContact.SiteLink;
            Instagram = dto.ShopContact.InstagramLink;
        }

        Schedule.Clear();
        HasSchedule = dto.Schedules is { Count: > 0 };
        if (dto.Schedules is not null)
        {
            foreach (var s in dto.Schedules.OrderBy(s => s.DayOfWeek))
                Schedule.Add(ScheduleLineViewModel.From(s));
        }

        Reviews.Clear();
        HasReviews = dto.Reviews is { Length: > 0 };
        if (dto.Reviews is not null)
        {
            foreach (var r in dto.Reviews)
                Reviews.Add(ShopReviewViewModel.From(r));
        }
    }

    private static string FormatPriceRange(PriceRange range) => new('$', (int)range);
}
