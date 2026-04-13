using Autofac;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
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
    }

    public override void OnFrameworkInitializationCompleted()
    {
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
             singleView.MainView = new MainView
             {
                 DataContext = mainViewModel
             };
        }

        base.OnFrameworkInitializationCompleted();
    }
}