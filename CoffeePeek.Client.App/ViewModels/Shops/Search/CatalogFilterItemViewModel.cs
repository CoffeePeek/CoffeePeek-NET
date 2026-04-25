using CoffeePeek.Client.App.ViewModels.Abstract;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CoffeePeek.Client.App.ViewModels.Shops.Search;

public partial class CatalogFilterItemViewModel : ViewModelBase
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }

    [ObservableProperty]
    public partial bool IsSelected { get; set; }
}
