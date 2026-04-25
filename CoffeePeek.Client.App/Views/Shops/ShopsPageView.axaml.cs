using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace CoffeePeek.Client.App.Views.Shops;

public partial class ShopsPageView : UserControl
{
    public ShopsPageView()
    {
        InitializeComponent();
        Padding = OperatingSystem.IsAndroid()
            ? new Thickness(0, 0, 0, 8)
            : new Thickness(24);
    }
}