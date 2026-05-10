using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;

namespace CoffeePeek.Client.App.Controls;

public partial class WobbleRingLoader : UserControl
{
    private DispatcherTimer? _timer;
    private RotateTransform? _rotate;
    private DateTime _start;

    public WobbleRingLoader()
    {
        InitializeComponent();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _rotate = new RotateTransform(0);
        Ring.RenderTransform = _rotate;
        _start = DateTime.UtcNow;
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _timer.Tick += OnTick;
        _timer.Start();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _timer?.Stop();
        _timer = null;
    }

    private void OnTick(object? sender, EventArgs e)
    {
        if (_rotate is null) return;
        _rotate.Angle = (DateTime.UtcNow - _start).TotalSeconds / 1.6 * 360 % 360;
    }
}
