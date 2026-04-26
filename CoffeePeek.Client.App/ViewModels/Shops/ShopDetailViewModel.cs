using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CoffeePeek.Client.App.Infrastructure.HTTP.Configuration;
using CoffeePeek.Client.App.Infrastructure.HTTP.WebClients;
using CoffeePeek.Client.App.Services;
using CoffeePeek.Client.App.ViewModels.Abstract;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Dtos.Schedule;
using CoffeePeek.Contract.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lang = CoffeePeek.Client.App.Resources.Lang.Resources;

namespace CoffeePeek.Client.App.ViewModels.Shops;

public partial class ShopDetailViewModel : ViewModelBase
{
    private const int MaxAmenityLabels = 4;

    private readonly IWebCoffeeShopsClient _shopsClient;
    private readonly IWorkspaceShellNavigator _shellNavigator;
    private readonly HttpClient _httpClient;
    private readonly ApiOptions _apiOptions;

    private Bitmap? _coverBitmap;
    private CancellationTokenSource? _heroLoadCts;

    public ShopDetailViewModel(
        IWebCoffeeShopsClient shopsClient,
        IWorkspaceShellNavigator shellNavigator,
        HttpClient httpClient,
        ApiOptions apiOptions)
    {
        _shopsClient = shopsClient;
        _shellNavigator = shellNavigator;
        _httpClient = httpClient;
        _apiOptions = apiOptions;
        Reviews.CollectionChanged += OnReviewsCollectionChanged;
    }

    private void OnReviewsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) =>
        OnPropertyChanged(nameof(HasReviewItems));

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
    public partial string RatingSummary { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string OpenUntilText { get; set; } = string.Empty;

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

    /// <summary>True when shop details (not only reviews) are ready to show after a successful load.</summary>
    [ObservableProperty]
    public partial bool IsDetailContentVisible { get; set; }

    public bool HasReviewItems => Reviews.Count > 0;

    [ObservableProperty]
    public partial bool CanCreateReview { get; set; }

    [ObservableProperty]
    public partial bool HasAmenities { get; set; }

    [ObservableProperty]
    public partial bool HasMapCoordinates { get; set; }

    [ObservableProperty]
    public partial string? Phone { get; set; }

    [ObservableProperty]
    public partial string? Email { get; set; }

    [ObservableProperty]
    public partial string? Website { get; set; }

    [ObservableProperty]
    public partial string? Instagram { get; set; }

    public Bitmap? CoverBitmap
    {
        get => _coverBitmap;
        private set
        {
            if (ReferenceEquals(_coverBitmap, value))
                return;

            _coverBitmap?.Dispose();
            _coverBitmap = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasHeroImage));
        }
    }

    public bool HasHeroImage => CoverBitmap is not null;

    public ObservableCollection<string> Tags { get; } = [];
    public ObservableCollection<string> AmenityLabels { get; } = [];
    public ObservableCollection<ScheduleLineViewModel> Schedule { get; } = [];
    public ObservableCollection<ShopReviewViewModel> Reviews { get; } = [];

    partial void OnErrorMessageChanged(string? value) => OnPropertyChanged(nameof(HasError));

    public async Task LoadAsync(Guid shopId, CancellationToken cancellationToken = default)
    {
        IsLoading = true;
        ErrorMessage = null;
        IsDetailContentVisible = false;
        CancelHeroLoad();
        await Dispatcher.UIThread.InvokeAsync(() => CoverBitmap = null);

        try
        {
            var result = await _shopsClient.GetByIdAsync(shopId, cancellationToken);

            if (result.IsFailed)
            {
                ErrorMessage = result.Errors.FirstOrDefault()?.Message ?? Lang.ShopDetail_LoadError;
                return;
            }

            var shop = result.Value.ShopDto;
            MapFromDto(shop);
            IsDetailContentVisible = true;

            var photoUrl = shop.Photos is { Length: > 0 } p && !string.IsNullOrWhiteSpace(p[0].FullUrl)
                ? p[0].FullUrl
                : null;
            _ = LoadHeroAsync(photoUrl, cancellationToken);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void GoBack() => _shellNavigator.CloseShopDetail();

    [RelayCommand]
    private void GetDirections()
    {
        string? url = null;
        if (HasMapCoordinates && Latitude is { } lat && Longitude is { } lng)
        {
            var latStr = lat.ToString(CultureInfo.InvariantCulture);
            var lngStr = lng.ToString(CultureInfo.InvariantCulture);
            url = $"https://www.google.com/maps/dir/?api=1&destination={Uri.EscapeDataString(latStr + "," + lngStr)}";
        }
        else if (HasAddress && !string.IsNullOrWhiteSpace(Address))
        {
            url = "https://www.google.com/maps/search/?api=1&query=" + Uri.EscapeDataString(Address.Trim());
        }

        if (url is not null)
            OpenExternalUri(url);
    }

    [RelayCommand]
    private void ShareShop() => OpenWebsiteIfAny();

    [RelayCommand]
    private void OrderOrCall()
    {
        if (!string.IsNullOrWhiteSpace(Phone))
        {
            var digits = new string(Phone.Where(char.IsDigit).ToArray());
            if (digits.Length > 0)
                OpenExternalUri("tel:" + digits);
            return;
        }

        OpenWebsiteIfAny();
    }

    [RelayCommand(CanExecute = nameof(CanWriteReview))]
    private void WriteReview()
    {
    }

    private bool CanWriteReview() => CanCreateReview;

    partial void OnCanCreateReviewChanged(bool value) => WriteReviewCommand.NotifyCanExecuteChanged();

    private void OpenWebsiteIfAny()
    {
        if (string.IsNullOrWhiteSpace(Website))
            return;

        var link = Website.Trim();
        if (!link.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            link = "https://" + link;

        OpenExternalUri(link);
    }

    private decimal? Latitude { get; set; }
    private decimal? Longitude { get; set; }

    private void MapFromDto(CoffeeShopDetailsDto dto)
    {
        Name = dto.Name;
        Description = dto.Description;
        HasDescription = !string.IsNullOrWhiteSpace(dto.Description);
        Rating = (double)dto.Rating;
        RatingLabel = dto.Rating.ToString("0.0", CultureInfo.InvariantCulture);
        ReviewCount = dto.ReviewCount;
        RatingSummary = ReviewCount > 0
            ? $"{RatingLabel} ({ReviewCount})"
            : RatingLabel;
        IsOpen = dto.IsOpen;
        IsNew = dto.IsNew;
        IsFavorite = dto.IsFavorite;
        CanCreateReview = dto.CanCreateReview == true;
        WriteReviewCommand.NotifyCanExecuteChanged();

        PriceLabel = dto.PriceRange switch
        {
            PriceRange.Cheap => "$",
            PriceRange.Moderate => "$$",
            PriceRange.Expensive => "$$$",
            PriceRange.Luxury => "$$$$",
            _ => ""
        };

        Latitude = null;
        Longitude = null;
        if (dto.Location is not null)
        {
            Address = dto.Location.Address;
            HasAddress = !string.IsNullOrWhiteSpace(dto.Location.Address);
            Latitude = dto.Location.Latitude;
            Longitude = dto.Location.Longitude;
            HasMapCoordinates = Latitude is not null && Longitude is not null;
        }
        else
        {
            Address = null;
            HasAddress = false;
            HasMapCoordinates = false;
        }

        Tags.Clear();
        void AddTag(string? s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return;
            Tags.Add(s.Trim());
        }

        if (dto.Roasters is not null)
            foreach (var r in dto.Roasters)
                AddTag(r.Name);
        if (dto.CoffeeBeans is not null)
            foreach (var b in dto.CoffeeBeans)
                AddTag(b.Name);
        if (dto.BrewMethods is not null)
            foreach (var m in dto.BrewMethods)
                AddTag(m.Name);

        AmenityLabels.Clear();
        if (dto.Equipments is not null)
        {
            foreach (var e in dto.Equipments.Take(MaxAmenityLabels))
            {
                if (!string.IsNullOrWhiteSpace(e.Name))
                    AmenityLabels.Add(e.Name.Trim());
            }
        }

        HasAmenities = AmenityLabels.Count > 0;

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

        OpenUntilText = BuildOpenUntilText(dto.Schedules);

        Reviews.Clear();
        if (dto.Reviews is not null)
        {
            foreach (var r in dto.Reviews)
                Reviews.Add(ShopReviewViewModel.From(r));
        }

        OnPropertyChanged(nameof(HasReviewItems));
        OnPropertyChanged(nameof(CanGetDirections));
        OnPropertyChanged(nameof(CanShare));
        OnPropertyChanged(nameof(CanOrder));
        OnPropertyChanged(nameof(QuickAddressLine));
    }

    public bool CanGetDirections => HasAddress || HasMapCoordinates;

    public bool CanShare => !string.IsNullOrWhiteSpace(Website);

    public bool CanOrder =>
        !string.IsNullOrWhiteSpace(Phone) || !string.IsNullOrWhiteSpace(Website);

    public string QuickAddressLine =>
        HasAddress && !string.IsNullOrWhiteSpace(Address) ? Address!.Trim() : "—";

    private static string BuildOpenUntilText(List<ScheduleDto>? schedules)
    {
        if (schedules is null || schedules.Count == 0)
            return "—";

        var today = DateTime.Now.DayOfWeek;
        var todaySchedule = schedules.FirstOrDefault(s => s.DayOfWeek == today);
        if (todaySchedule is null)
            return "—";

        if (todaySchedule.IsClosed)
            return Lang.ShopDetail_ClosedToday;

        var intervals = todaySchedule.Intervals;
        if (intervals is null || intervals.Count == 0)
            return Lang.ShopDetail_ClosedToday;

        var lastClose = intervals.Max(i => i.CloseTime);
        var formatted = DateTime.Today.Add(lastClose).ToString("HH:mm", CultureInfo.CurrentCulture);
        return string.Format(CultureInfo.CurrentCulture, Lang.ShopDetail_OpenUntil, formatted);
    }

    private async Task LoadHeroAsync(string? imageUrl, CancellationToken parentCancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            return;

        CancelHeroLoad();
        var linked = CancellationTokenSource.CreateLinkedTokenSource(parentCancellationToken);
        _heroLoadCts = linked;
        var ct = linked.Token;

        try
        {
            var uri = ResolveImageUri(imageUrl, _apiOptions);
            if (uri is null)
                return;

            using var response = await _httpClient.GetAsync(uri, ct).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            await using var stream = await response.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
            await using var ms = new MemoryStream();
            await stream.CopyToAsync(ms, ct).ConfigureAwait(false);
            ms.Position = 0;

            if (ct.IsCancellationRequested)
                return;

            var bitmap = new Bitmap(ms);
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (!ct.IsCancellationRequested)
                    CoverBitmap = bitmap;
                else
                    bitmap.Dispose();
            });
        }
        catch (OperationCanceledException)
        {
            // Navigation or a newer load cancelled this request.
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ShopDetail hero load failed: {ex}");
        }
    }

    private void CancelHeroLoad()
    {
        _heroLoadCts?.Cancel();
        _heroLoadCts?.Dispose();
        _heroLoadCts = null;
    }

    private static Uri? ResolveImageUri(string url, ApiOptions apiOptions)
    {
        if (Uri.TryCreate(url, UriKind.Absolute, out var absolute))
            return absolute;

        var baseAddr = apiOptions.BaseAddress.TrimEnd('/');
        var path = url.TrimStart('/');
        return Uri.TryCreate($"{baseAddr}/{path}", UriKind.Absolute, out var relative)
            ? relative
            : null;
    }

    private static void OpenExternalUri(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return;

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return;

        Dispatcher.UIThread.Post(() =>
        {
            try
            {
                if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                    && desktop.MainWindow is { } w)
                {
                    _ = w.Launcher.LaunchUriAsync(uri);
                    return;
                }
            }
            catch
            {
                // fall through
            }

            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch
            {
                // ignored
            }
        });
    }
}
