using System.ComponentModel;
using Avalonia.Controls;
using CoffeePeek.Client.App.ViewModels.Shops;

namespace CoffeePeek.Client.App.Views.Shops;

public partial class ShopDetailView : UserControl
{
    public ShopDetailView() => InitializeComponent();

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is ShopDetailViewModel vm)
        {
            vm.PropertyChanged -= OnVmPropertyChanged;
            vm.PropertyChanged += OnVmPropertyChanged;
            ApplyResponsiveColumns(vm.IsCompactLayout);
        }
    }

    private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(ShopDetailViewModel.IsCompactLayout))
            return;

        if (sender is ShopDetailViewModel vm)
            ApplyResponsiveColumns(vm.IsCompactLayout);
    }

    private void ApplyResponsiveColumns(bool compact)
    {
        ConfigureGrid(DetailSkeletonRootGrid, compact);
        ConfigureGrid(DetailBodyRootGrid, compact);
    }

    private static void ConfigureGrid(Grid grid, bool compact)
    {
        grid.ColumnDefinitions.Clear();
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        if (!compact)
            grid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(310)));

        grid.ColumnSpacing = compact ? 0 : 24;
    }
}
