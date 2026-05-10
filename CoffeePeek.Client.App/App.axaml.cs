using Autofac;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CoffeePeek.Client.App.Core.Execution;
using CoffeePeek.Client.App.ViewModels;
using CoffeePeek.Client.App.ViewModels.Mobile;
using CoffeePeek.Client.App.Views;
using CoffeePeek.Client.App.Views.Mobile;

namespace CoffeePeek.Client.App;

public partial class App : Application
{
    public IContainer Services => _container;

    private IContainer _container = null!;
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        _container = Bootstrapper.BuildContainer();
        _ = _container.Resolve<IApplicationExecutorRunner>().RunAfterInitAsync();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        _ = _container.Resolve<IApplicationExecutorRunner>().RunBeforeMainShellAsync();

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
            if (OperatingSystem.IsAndroid())
            {
                var mobileRoot = new MobileRootView { DataContext = mainViewModel };
                mobileRoot.SetMobileShellViewModel(_container.Resolve<MobileShellViewModel>());
                singleView.MainView = mobileRoot;
            }
            else
            {
                singleView.MainView = new MainView { DataContext = mainViewModel };
            }
        }

        base.OnFrameworkInitializationCompleted();

        _ = _container.Resolve<IApplicationExecutorRunner>().RunAfterStartupAsync();
    }
}