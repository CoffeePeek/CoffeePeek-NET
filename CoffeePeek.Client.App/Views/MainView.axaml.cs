using Autofac;
using Avalonia;
using Avalonia.Controls;
using CoffeePeek.Client.App.Services;

namespace CoffeePeek.Client.App.Views;

public partial class MainView : UserControl
{
    private ILayoutBreakpointService? _layout;
    private TopLevel? _topLevel;
    private bool _listening;

    public MainView()
    {
        InitializeComponent();
        AttachedToVisualTree += OnAttachedToVisualTree;
        DetachedFromVisualTree += OnDetachedFromVisualTree;
    }

    private void OnAttachedToVisualTree(object? sender, Avalonia.VisualTreeAttachmentEventArgs e)
    {
        if (_listening)
            return;

        if (Application.Current is not App app)
            return;

        _layout ??= app.Services.Resolve<ILayoutBreakpointService>();

        var top = TopLevel.GetTopLevel(this);
        if (top is null)
            return;

        _topLevel = top;
        _topLevel.LayoutUpdated += OnTopLayoutUpdated;
        _listening = true;
        PushViewport();
    }

    private void OnDetachedFromVisualTree(object? sender, Avalonia.VisualTreeAttachmentEventArgs e)
    {
        if (!_listening || _topLevel is null)
            return;

        _topLevel.LayoutUpdated -= OnTopLayoutUpdated;
        _topLevel = null;
        _listening = false;
    }

    private void OnTopLayoutUpdated(object? sender, EventArgs e) =>
        PushViewport();

    private void PushViewport()
    {
        if (_layout is null)
            return;

        var w = _topLevel?.ClientSize.Width ?? 0;
        if (w <= 1)
            w = Bounds.Width;

        _layout.UpdateViewportWidth(w);
    }
}
