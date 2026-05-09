using Autofac;
using Avalonia;
using Avalonia.Controls;
using CoffeePeek.Client.App.Services;

namespace CoffeePeek.Client.App.Views;

public partial class MainWindow : Window
{
    private ILayoutBreakpointService? _layout;

    public MainWindow()
    {
        InitializeComponent();
        Opened += (_, _) =>
            _layout ??= (Application.Current as App)?.Services.Resolve<ILayoutBreakpointService>();

        LayoutUpdated += (_, _) =>
        {
            _layout ??= (Application.Current as App)?.Services.Resolve<ILayoutBreakpointService>();
            _layout?.UpdateViewportWidth(ClientSize.Width);
        };
    }
}
