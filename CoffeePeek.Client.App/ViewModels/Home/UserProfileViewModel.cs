using System.Collections.ObjectModel;
using System.Globalization;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CoffeePeek.Client.App.Infrastructure.HTTP.Configuration;
using CoffeePeek.Client.App.Infrastructure.HTTP.WebClients;
using CoffeePeek.Client.App.Services;
using CoffeePeek.Client.App.Core.Identity;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Client.App.ViewModels.Abstract;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lang = CoffeePeek.Client.App.Resources.Lang.Resources;

namespace CoffeePeek.Client.App.ViewModels.Home;

public partial class UserProfileViewModel : ViewModelBase
{
    private CancellationTokenSource? _loadCts;
    private CancellationTokenSource? _avatarUploadCts;
    private Guid? _userId;

    public UserProfileViewModel(
        HttpClient httpClient,
        ApiOptions apiOptions,
        IWebUserProfileClient profileClient,
        IWebUserReviewsClient reviewsClient,
        IUserIdentityAccessor identityAccessor,
        IImagePickerService imagePickerService,
        ILayoutBreakpointService layoutBreakpoints)
    {
        HttpClient = httpClient;
        ApiOptions = apiOptions;
        ProfileClient = profileClient;
        ReviewsClient = reviewsClient;
        IdentityAccessor = identityAccessor;
        ImagePickerService = imagePickerService;
        LayoutBreakpoints = layoutBreakpoints;

        layoutBreakpoints.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName != nameof(ILayoutBreakpointService.IsCompact))
                return;

            OnPropertyChanged(nameof(ProfileHeaderPadding));
            OnPropertyChanged(nameof(ProfileStatsSectionMargin));
            OnPropertyChanged(nameof(ProfileReviewsSectionMargin));
            OnPropertyChanged(nameof(ProfileEditFormHorizontalMargin));
        };
    }

    private HttpClient HttpClient { get; }
    private ApiOptions ApiOptions { get; }
    private IWebUserProfileClient ProfileClient { get; }
    private IWebUserReviewsClient ReviewsClient { get; }
    private IUserIdentityAccessor IdentityAccessor { get; }
    private IImagePickerService ImagePickerService { get; }
    private ILayoutBreakpointService LayoutBreakpoints { get; }

    public Thickness ProfileHeaderPadding =>
        LayoutBreakpoints.IsCompact ? new Thickness(24, 28) : new Thickness(48, 40);

    public Thickness ProfileStatsSectionMargin =>
        LayoutBreakpoints.IsCompact ? new Thickness(16, 24) : new Thickness(48, 32);

    public Thickness ProfileReviewsSectionMargin =>
        LayoutBreakpoints.IsCompact ? new Thickness(16, 0) : new Thickness(48, 0);

    public Thickness ProfileEditFormHorizontalMargin =>
        LayoutBreakpoints.IsCompact ? new Thickness(16, 0) : new Thickness(48, 0);

    [ObservableProperty]
    public partial bool IsLoadingProfile { get; set; }

    [ObservableProperty]
    public partial bool IsLoadingReviews { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial string UserName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? About { get; set; }

    [ObservableProperty]
    public partial bool IsOwnProfile { get; set; }

    [ObservableProperty]
    public partial bool IsEditing { get; set; }

    [ObservableProperty]
    public partial bool IsSaving { get; set; }

    [ObservableProperty]
    public partial bool IsUploadingAvatar { get; set; }

    [ObservableProperty]
    public partial string? EditErrorMessage { get; set; }

    public bool HasEditError => !string.IsNullOrEmpty(EditErrorMessage);

    [ObservableProperty]
    public partial string? AvatarUploadErrorMessage { get; set; }

    public bool HasAvatarUploadError => !string.IsNullOrEmpty(AvatarUploadErrorMessage);

    [ObservableProperty]
    public partial string EditUserName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string EditAbout { get; set; } = string.Empty;

    [ObservableProperty]
    public partial int MemberSinceYear { get; set; }

    [ObservableProperty]
    public partial bool HasMemberSince { get; set; }

    [ObservableProperty]
    public partial string MemberSinceLabel { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string InitialLetter { get; set; } = "?";

    [ObservableProperty]
    public partial bool ShowProfileBody { get; set; }

    [ObservableProperty]
    public partial bool ShowReviewsEmpty { get; set; }

    [ObservableProperty]
    public partial int CheckInCount { get; set; }

    [ObservableProperty]
    public partial int ReviewCount { get; set; }

    [ObservableProperty]
    public partial string ReviewsSubtitle { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AverageRatingLabel { get; set; } = "0.0";

    [ObservableProperty]
    public partial Bitmap? AvatarImage { get; set; }

    [ObservableProperty]
    public partial bool HasAvatar { get; set; }

    public ObservableCollection<UserProfileReviewRowViewModel> Reviews { get; } = [];

    [ObservableProperty]
    public partial int ReviewsPage { get; set; } = 1;

    [ObservableProperty]
    public partial int ReviewsTotalPages { get; set; } = 1;

    [ObservableProperty]
    public partial bool CanReviewPrev { get; set; }

    [ObservableProperty]
    public partial bool CanReviewNext { get; set; }

    [ObservableProperty]
    public partial bool HasReviews { get; set; }

    [ObservableProperty]
    public partial bool ShowReviewPagination { get; set; }

    partial void OnEditErrorMessageChanged(string? value) => OnPropertyChanged(nameof(HasEditError));

    partial void OnAvatarUploadErrorMessageChanged(string? value) => OnPropertyChanged(nameof(HasAvatarUploadError));

    public async Task LoadAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _userId = userId;

        _loadCts?.Cancel();
        _loadCts?.Dispose();
        _loadCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _avatarUploadCts?.Cancel();
        _avatarUploadCts?.Dispose();
        _avatarUploadCts = null;
        var ct = _loadCts.Token;

        var currentUserId = IdentityAccessor.GetCurrentUserIdOrNull();

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            IsLoadingProfile = true;
            ErrorMessage = null;
            ShowProfileBody = false;
            IsEditing = false;
            IsSaving = false;
            IsUploadingAvatar = false;
            EditErrorMessage = null;
            AvatarUploadErrorMessage = null;
            IsOwnProfile = currentUserId.HasValue && currentUserId.Value == userId;
            Reviews.Clear();
            HasReviews = false;
            ShowReviewsEmpty = false;
            ShowReviewPagination = false;
            ReviewsPage = 1;
            ReviewsTotalPages = 1;
            UpdateReviewNav();
            DisposeAvatar();
        });

        var profileResult = await ProfileClient.GetPublicProfileAsync(userId, ct);
        if (ct.IsCancellationRequested)
            return;

        if (profileResult.IsFailed || profileResult.Value is null)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                ErrorMessage = profileResult.Errors.FirstOrDefault()?.Message
                    ?? Resources.Lang.Resources.Profile_LoadError;
                IsLoadingProfile = false;
            });
            return;
        }

        var p = profileResult.Value;
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            UserName = p.UserName;
            About = p.About;
            CheckInCount = p.CheckInCount;
            ReviewCount = p.ReviewCount;
            ReviewsSubtitle = p.ReviewCount > 0
                ? string.Format(Resources.Lang.Resources.Profile_ReviewsTotal, p.ReviewCount)
                : Resources.Lang.Resources.Profile_NoReviewsYet;
            HasMemberSince = p.CreatedAtUtc != default;
            MemberSinceYear = p.CreatedAtUtc.Year;
            MemberSinceLabel = string.Format(
                Resources.Lang.Resources.Profile_MemberSince,
                p.CreatedAtUtc.Year);
            InitialLetter = string.IsNullOrEmpty(p.UserName)
                ? "?"
                : char.ToUpperInvariant(p.UserName[0]).ToString();
            IsLoadingProfile = false;
            ShowProfileBody = true;
        });

        await LoadAvatarIfNeededAsync(p.AvatarUrl, ct);
        if (ct.IsCancellationRequested)
            return;

        await LoadReviewsPageAsync(userId, page: 1, ct);
    }

    private async Task LoadReviewsPageAsync(Guid userId, int page, CancellationToken ct)
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            IsLoadingReviews = true;
            ShowReviewsEmpty = false;
        });

        var reviewsResult = await ReviewsClient.GetReviewsAsync(userId, page, 10, ct);
        if (ct.IsCancellationRequested)
            return;

        if (reviewsResult.IsFailed || reviewsResult.Value is null)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Reviews.Clear();
                IsLoadingReviews = false;
                HasReviews = false;
                ShowReviewsEmpty = true;
            });
            return;
        }

        var data = reviewsResult.Value;
        var rows = (data.ReviewDtos ?? [])
            .Select(MapReview)
            .ToList();

        var avg = rows.Count > 0
            ? rows.Average(r => r.AverageValue)
            : 0d;

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            Reviews.Clear();
            foreach (var r in rows)
                Reviews.Add(r);

            ReviewsPage = data.CurrentPage > 0 ? data.CurrentPage : page;
            ReviewsTotalPages = Math.Max(1, data.TotalPages);
            HasReviews = rows.Count > 0;
            ShowReviewsEmpty = rows.Count == 0;
            ShowReviewPagination = ReviewsTotalPages > 1;
            AverageRatingLabel = avg.ToString("0.0", CultureInfo.InvariantCulture);
            IsLoadingReviews = false;
            UpdateReviewNav();
        });
    }

    private UserProfileReviewRowViewModel MapReview(ReviewDto dto)
    {
        var r = dto.Rating;
        var coffee = r?.Coffee ?? 0;
        var service = r?.Service ?? 0;
        var place = r?.Place ?? 0;
        var avg = (coffee + service + place) / 3.0;
        var stars = (int)Math.Round(avg, MidpointRounding.AwayFromZero);
        stars = Math.Clamp(stars, 0, 5);

        var date = dto.CreatedAtUtc.ToLocalTime();
        var formatted = date.ToString("d MMMM yyyy", CultureInfo.GetCultureInfo("ru-RU"));

        var title = string.IsNullOrWhiteSpace(dto.Header)
            ? Resources.Lang.Resources.Profile_ReviewDefaultTitle
            : dto.Header;

        return new UserProfileReviewRowViewModel(
            title,
            dto.Comment,
            formatted,
            stars,
            avg,
            dto.CoffeeShopId,
            OpenShop);
    }

    private async Task LoadAvatarIfNeededAsync(string? avatarUrl, CancellationToken ct)
    {
        await Dispatcher.UIThread.InvokeAsync(DisposeAvatar);

        var uri = ResolveAvatarUri(avatarUrl);
        if (uri is null)
            return;

        try
        {
            using var response = await HttpClient.GetAsync(uri, ct);
            response.EnsureSuccessStatusCode();
            await using var stream = await response.Content.ReadAsStreamAsync(ct);
            await using var ms = new MemoryStream();
            await stream.CopyToAsync(ms, ct);
            ms.Position = 0;

            if (ct.IsCancellationRequested)
                return;

            var bitmap = new Bitmap(ms);
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                DisposeAvatar();
                AvatarImage = bitmap;
                HasAvatar = true;
            });
        }
        catch
        {
            // ignore avatar failures
        }
    }

    private Uri? ResolveAvatarUri(string? avatarUrl)
    {
        if (string.IsNullOrWhiteSpace(avatarUrl))
            return null;

        if (Uri.TryCreate(avatarUrl, UriKind.Absolute, out var absolute))
            return absolute;

        var baseAddr = ApiOptions.BaseAddress.TrimEnd('/');
        var path = avatarUrl.TrimStart('/');
        return Uri.TryCreate($"{baseAddr}/{path}", UriKind.Absolute, out var rel)
            ? rel
            : null;
    }

    public string PaginationSummary =>
        string.Format(Resources.Lang.Resources.Profile_PageOf, ReviewsPage, ReviewsTotalPages);

    partial void OnReviewsPageChanged(int value)
    {
        UpdateReviewNav();
        OnPropertyChanged(nameof(PaginationSummary));
    }

    partial void OnReviewsTotalPagesChanged(int value)
    {
        UpdateReviewNav();
        OnPropertyChanged(nameof(PaginationSummary));
    }

    private void UpdateReviewNav()
    {
        CanReviewPrev = ReviewsPage > 1;
        CanReviewNext = ReviewsPage < ReviewsTotalPages;
    }

    [RelayCommand]
    private async Task ReviewPrevAsync()
    {
        if (_userId is null || ReviewsPage <= 1 || _loadCts is null)
            return;

        var nextPage = ReviewsPage - 1;
        _loadCts.Cancel();
        _loadCts.Dispose();
        _loadCts = new CancellationTokenSource();
        await LoadReviewsPageAsync(_userId.Value, nextPage, _loadCts.Token);
    }

    [RelayCommand]
    private async Task ReviewNextAsync()
    {
        if (_userId is null || ReviewsPage >= ReviewsTotalPages || _loadCts is null)
            return;

        var nextPage = ReviewsPage + 1;
        _loadCts.Cancel();
        _loadCts.Dispose();
        _loadCts = new CancellationTokenSource();
        await LoadReviewsPageAsync(_userId.Value, nextPage, _loadCts.Token);
    }

    [RelayCommand]
    private void StartEditing()
    {
        EditUserName = UserName;
        EditAbout = About ?? string.Empty;
        EditErrorMessage = null;
        IsEditing = true;
    }

    [RelayCommand]
    private void CancelEditing()
    {
        IsEditing = false;
        EditErrorMessage = null;
    }

    [RelayCommand]
    private async Task SaveProfileAsync()
    {
        if (IsSaving) return;

        IsSaving = true;
        EditErrorMessage = null;

        try
        {
            var usernameChanged = !string.Equals(EditUserName.Trim(), UserName, StringComparison.Ordinal);
            var aboutChanged = !string.Equals(EditAbout.Trim(), About ?? string.Empty, StringComparison.Ordinal);

            if (usernameChanged)
            {
                var trimmed = EditUserName.Trim();
                if (string.IsNullOrWhiteSpace(trimmed))
                {
                    EditErrorMessage = Lang.Profile_Edit_UsernameRequired;
                    return;
                }

                var result = await ProfileClient.UpdateUsernameAsync(trimmed);
                if (result.IsFailed)
                {
                    EditErrorMessage = result.Errors.FirstOrDefault()?.Message ?? Lang.Profile_Edit_SaveError;
                    return;
                }

                UserName = trimmed;
                InitialLetter = char.ToUpperInvariant(trimmed[0]).ToString();
            }

            if (aboutChanged)
            {
                var result = await ProfileClient.UpdateAboutAsync(EditAbout.Trim());
                if (result.IsFailed)
                {
                    EditErrorMessage = result.Errors.FirstOrDefault()?.Message ?? Lang.Profile_Edit_SaveError;
                    return;
                }

                About = EditAbout.Trim();
            }

            IsEditing = false;
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand(AllowConcurrentExecutions = false)]
    private async Task UploadAvatarAsync()
    {
        if (!IsOwnProfile || _userId is null)
            return;

        _avatarUploadCts?.Cancel();
        _avatarUploadCts?.Dispose();
        _avatarUploadCts = CancellationTokenSource.CreateLinkedTokenSource(_loadCts?.Token ?? CancellationToken.None);
        var ct = _avatarUploadCts.Token;

        AvatarUploadErrorMessage = null;
        IsUploadingAvatar = true;

        try
        {
            var picked = await ImagePickerService.PickImageAsync(ct);
            if (picked is null)
                return;

            var uploadResult = await ProfileClient.UploadAvatarAsync(
                picked.FileName,
                picked.ContentType,
                picked.Content,
                ct);

            if (uploadResult.IsFailed)
            {
                AvatarUploadErrorMessage = uploadResult.Errors.FirstOrDefault()?.Message
                    ?? Lang.Profile_AvatarUploadError;
                return;
            }

            // Reload must not use the avatar upload token: LoadAsync cancels _loadCts, which
            // would cascade-cancel a linked _avatarUploadCts and skip the profile refresh.
            await LoadAsync(_userId.Value);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            AvatarUploadErrorMessage = Lang.Profile_AvatarUploadError;
        }
        finally
        {
            IsUploadingAvatar = false;
        }
    }

    [RelayCommand]
    private void OpenShop(Guid coffeeShopId)
    {
        _ = coffeeShopId;
    }

    private void DisposeAvatar()
    {
        if (AvatarImage is null)
            return;

        AvatarImage.Dispose();
        AvatarImage = null;
        HasAvatar = false;
    }
}

public sealed partial class UserProfileReviewRowViewModel : ObservableObject
{
    public UserProfileReviewRowViewModel(
        string title,
        string? comment,
        string formattedDate,
        int starCount,
        double averageValue,
        Guid coffeeShopId,
        Action<Guid> goToShop)
    {
        Title = title;
        Comment = comment;
        FormattedDate = formattedDate;
        StarCount = starCount;
        AverageValue = averageValue;
        CoffeeShopId = coffeeShopId;
        _goToShop = goToShop;
    }

    public string Title { get; }
    public string? Comment { get; }
    public string FormattedDate { get; }
    public int StarCount { get; }
    public double AverageValue { get; }
    public Guid CoffeeShopId { get; }

    private readonly Action<Guid> _goToShop;

    public string StarsText => new string('★', StarCount) + new string('☆', 5 - StarCount);

    [RelayCommand]
    private void GoToShop() => _goToShop(CoffeeShopId);
}
