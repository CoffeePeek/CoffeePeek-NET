using System.Collections.ObjectModel;
using CoffeePeek.Client.App.ViewModels.Abstract;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CoffeePeek.Client.App.ViewModels.Shops;

public partial class ShopCardViewModel : ViewModelBase
{
    public Guid Id { get; init; }

    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    [ObservableProperty]
    public partial double Rating { get; set; }

    [ObservableProperty]
    public partial int ReviewCount { get; set; }

    [ObservableProperty]
    public partial bool IsTrending { get; set; }

    [ObservableProperty]
    public partial bool IsOpen { get; set; }

    [ObservableProperty]
    public partial bool IsFavorite { get; set; }

    public ObservableCollection<string> Tags { get; } = [];

    public static ShopCardViewModel FromDto(ShortShopDto dto)
    {
        var vm = new ShopCardViewModel
        {
            Id = dto.Id,
            Name = dto.Name,
            Rating = (double)dto.Rating,
            ReviewCount = dto.ReviewCount,
            IsTrending = dto.IsNew,
            IsOpen = dto.IsOpen,
            IsFavorite = dto.IsFavorite
        };

        if (dto.Roasters is not null)
            foreach (var r in dto.Roasters) vm.Tags.Add(r.Name);

        if (dto.Beans is not null)
            foreach (var b in dto.Beans) vm.Tags.Add(b.Name);

        if (dto.BrewMethods is not null)
            foreach (var m in dto.BrewMethods) vm.Tags.Add(m.Name);

        return vm;
    }
}
