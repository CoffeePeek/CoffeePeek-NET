using System.Collections.ObjectModel;
using System.Net.Http;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CoffeePeek.Client.App.Infrastructure.HTTP.Configuration;
using CoffeePeek.Client.App.ViewModels.Abstract;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Enums;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CoffeePeek.Client.App.ViewModels.Shops;

public partial class ShopCardViewModel : ViewModelBase
{
    private Bitmap? _coverBitmap;

    public Guid Id { get; init; }

    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    [ObservableProperty]
    public partial double Rating { get; set; }

    [ObservableProperty]
    public partial int ReviewCount { get; set; }

    [ObservableProperty]
    public partial bool IsFavorite { get; set; }

    public string? CoverImageUrl { get; init; }

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
            OnPropertyChanged(nameof(HasCoverImage));
        }
    }

    public bool HasCoverImage => CoverBitmap is not null;

    [ObservableProperty]
    public partial string PriceLabel { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string LocationLine { get; set; } = string.Empty;

    public ObservableCollection<string> Tags { get; } = [];

    public void ReleaseCover() => CoverBitmap = null;

    public async Task LoadCoverAsync(HttpClient http, ApiOptions apiOptions, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(CoverImageUrl))
            return;

        try
        {
            var uri = ResolveImageUri(CoverImageUrl, apiOptions);
            if (uri is null)
                return;

            using var response = await http.GetAsync(uri, ct).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            await using var stream = await response.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
            await using var ms = new MemoryStream();
            await stream.CopyToAsync(ms, ct).ConfigureAwait(false);
            ms.Position = 0;

            if (ct.IsCancellationRequested)
                return;

            var bitmap = new Bitmap(ms);
            await Dispatcher.UIThread.InvokeAsync(() => CoverBitmap = bitmap);
        }
        catch
        {
            // Missing or invalid shop art should not break the grid.
        }
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

    private static string FormatPriceRange(PriceRange range) =>
        range switch
        {
            PriceRange.Cheap => "$",
            PriceRange.Moderate => "$$",
            PriceRange.Expensive => "$$$",
            PriceRange.Luxury => "$$$$",
            _ => string.Empty
        };

    public static ShopCardViewModel FromDto(ShortShopDto dto)
    {
        var vm = new ShopCardViewModel
        {
            Id = dto.Id,
            Name = dto.Name,
            Rating = (double)dto.Rating,
            ReviewCount = dto.ReviewCount,
            IsFavorite = dto.IsFavorite,
            CoverImageUrl = dto.Photos is { Length: > 0 } p ? p[0].FullUrl : null,
            PriceLabel = FormatPriceRange(dto.PriceRange),
            LocationLine = dto.Location?.Address?.Trim() ?? string.Empty
        };

        void TryAddTag(string? s)
        {
            if (vm.Tags.Count >= 2 || string.IsNullOrWhiteSpace(s))
                return;

            vm.Tags.Add(s.Trim().ToUpperInvariant());
        }

        if (dto.Roasters is not null)
        {
            foreach (var r in dto.Roasters)
                TryAddTag(r.Name);
        }

        if (dto.Beans is not null)
        {
            foreach (var b in dto.Beans)
                TryAddTag(b.Name);
        }

        if (dto.BrewMethods is not null)
        {
            foreach (var m in dto.BrewMethods)
                TryAddTag(m.Name);
        }

        return vm;
    }
}
