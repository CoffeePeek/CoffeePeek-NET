using CoffeePeek.Client.App.ViewModels.Abstract;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CoffeePeek.Client.App.ViewModels.Shops.Search;

public partial class CatalogFilterItemViewModel : ViewModelBase
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    [ObservableProperty]
    public partial bool IsSelected { get; set; }
}
