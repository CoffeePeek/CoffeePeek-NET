using Avalonia.Controls;
using CoffeePeek.Client.App.ViewModels.Mobile;

namespace CoffeePeek.Client.App.Views.Mobile;

public partial class MobileRootView : UserControl
{
    public MobileRootView()
    {
        InitializeComponent();
    }

    public void SetMobileShellViewModel(MobileShellViewModel vm)
    {
        MobileShell.DataContext = vm;
    }
}
