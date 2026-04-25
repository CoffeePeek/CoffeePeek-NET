using Autofac;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CoffeePeek.Client.App.Core.Execution;
using CoffeePeek.Client.App.ViewModels;
using CoffeePeek.Client.App.Views;

namespace CoffeePeek.Client.App;

public partial class App : Application
{
    private IContainer _container = null!;
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        _container = Bootstrapper.BuildContainer();
        _container.Resolve<IApplicationExecutorRunner>().RunAfterInitAsync().GetAwaiter().GetResult();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        _container.Resolve<IApplicationExecutorRunner>().RunBeforeMainShellAsync().GetAwaiter().GetResult();

        var mainViewModel = _container.Resolve<MainViewModel>();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleView)
        {
            var mainView = new MainView { DataContext = mainViewModel };
            // Android: inset from system bars / notch; keyboard handled by MainActivity (AdjustResize).
            if (OperatingSystem.IsAndroid())
                mainView.Padding = new Thickness(12, 28, 12, 12);
            singleView.MainView = mainView;
        }

        base.OnFrameworkInitializationCompleted();

        _container.Resolve<IApplicationExecutorRunner>().RunAfterStartupAsync().GetAwaiter().GetResult();
    }
}