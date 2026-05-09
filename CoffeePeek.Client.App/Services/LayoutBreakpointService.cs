using System;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CoffeePeek.Client.App.Services;

public sealed partial class LayoutBreakpointService : ObservableObject, ILayoutBreakpointService
{
    public const double CompactWidthThresholdPixels = 720;

    private double _viewportWidth;

    [ObservableProperty]
    public partial bool IsCompact { get; private set; }

    public Thickness WorkspaceChromeMargin =>
        OperatingSystem.IsAndroid()
            ? default
            : IsCompact
                ? new Thickness(12, 0, 12, 16)
                : new Thickness(32, 0, 32, 16);

    public void UpdateViewportWidth(double width)
    {
        if (width > 0 && !double.IsNaN(width) && !double.IsInfinity(width))
            _viewportWidth = width;

        var w = _viewportWidth;
        var compact = w > 0 && w < CompactWidthThresholdPixels;
        if (compact == IsCompact)
            return;

        IsCompact = compact;

    }

    partial void OnIsCompactChanged(bool oldValue, bool newValue) =>
        OnPropertyChanged(nameof(WorkspaceChromeMargin));
}
