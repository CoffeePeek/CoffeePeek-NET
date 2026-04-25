using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using CoffeePeek.Client.App.Core.Identity;
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
    private readonly IWebCoffeeShopReviewsClient _reviewsClient;
    private readonly IWorkspaceShellNavigator _shellNavigator;
    private readonly IUserIdentityAccessor _identityAccessor;
    private Guid? _shopId;

    public ShopDetailViewModel(
        IWebCoffeeShopsClient shopsClient,
        IWebCoffeeShopReviewsClient reviewsClient,
        IWorkspaceShellNavigator shellNavigator,
        IUserIdentityAccessor identityAccessor)
    {
        _shopsClient = shopsClient;
        _reviewsClient = reviewsClient;
        _shellNavigator = shellNavigator;
        _identityAccessor = identityAccessor;
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
    public partial bool CanCreateReview { get; set; }

    [ObservableProperty]
    public partial bool IsSubmittingReview { get; set; }

    [ObservableProperty]
    public partial bool IsDeletingReview { get; set; }

    [ObservableProperty]
    public partial string NewReviewComment { get; set; } = string.Empty;

    [ObservableProperty]
    public partial int PlaceScore { get; set; } = 5;

    [ObservableProperty]
    public partial int ServiceScore { get; set; } = 5;

    [ObservableProperty]
    public partial int CoffeeScore { get; set; } = 5;

    [ObservableProperty]
    public partial string? ReviewErrorMessage { get; set; }

    public bool HasReviewError => !string.IsNullOrWhiteSpace(ReviewErrorMessage);

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
    public IReadOnlyList<int> ScoreOptions { get; } = [1, 2, 3, 4, 5];

    partial void OnErrorMessageChanged(string? value) => OnPropertyChanged(nameof(HasError));
    partial void OnReviewErrorMessageChanged(string? value) => OnPropertyChanged(nameof(HasReviewError));

    public async Task LoadAsync(Guid shopId)
    {
        _shopId = shopId;
        IsLoading = true;
        ErrorMessage = null;
        ReviewErrorMessage = null;

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
            await LoadReviewPermissionsAsync(shopId);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void GoBack() => _shellNavigator.CloseShopDetail();

    [RelayCommand(AllowConcurrentExecutions = false)]
    private async Task SubmitReviewAsync()
    {
        if (_shopId is null || !CanCreateReview)
            return;

        IsSubmittingReview = true;
        ReviewErrorMessage = null;

        try
        {
            var createResult = await _reviewsClient.CreateAsync(
                _shopId.Value,
                NewReviewComment,
                PlaceScore,
                ServiceScore,
                CoffeeScore);

            if (createResult.IsFailed)
            {
                ReviewErrorMessage = createResult.Errors.FirstOrDefault()?.Message ?? Lang.ShopDetail_ReviewActionFailed;
                return;
            }

            await LoadAsync(_shopId.Value);
            NewReviewComment = string.Empty;
        }
        finally
        {
            IsSubmittingReview = false;
        }
    }

    [RelayCommand(AllowConcurrentExecutions = false)]
    private async Task DeleteReviewAsync(ShopReviewViewModel? review)
    {
        if (_shopId is null || review is null || !review.IsOwnReview)
            return;

        IsDeletingReview = true;
        ReviewErrorMessage = null;

        try
        {
            var result = await _reviewsClient.DeleteAsync(review.Id);
            if (result.IsFailed)
            {
                ReviewErrorMessage = result.Errors.FirstOrDefault()?.Message ?? Lang.ShopDetail_ReviewActionFailed;
                return;
            }

            await LoadAsync(_shopId.Value);
        }
        finally
        {
            IsDeletingReview = false;
        }
    }

    private void MapFromDto(CoffeeShopDetailsDto dto)
    {
        var currentUserId = _identityAccessor.GetCurrentUserIdOrNull();

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
                Reviews.Add(ShopReviewViewModel.From(r, currentUserId));
        }
    }

    private async Task LoadReviewPermissionsAsync(Guid shopId)
    {
        var permissionResult = await _reviewsClient.CanCreateAsync(shopId);
        if (permissionResult.IsFailed)
        {
            CanCreateReview = false;
            return;
        }

        CanCreateReview = permissionResult.Value.CanCreate;
    }
}
