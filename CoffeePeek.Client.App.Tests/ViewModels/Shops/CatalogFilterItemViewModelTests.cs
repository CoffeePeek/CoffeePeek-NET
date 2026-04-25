using CoffeePeek.Client.App.ViewModels.Shops.Search;
using FluentAssertions;
using Xunit;

namespace CoffeePeek.Client.App.Tests.ViewModels.Shops;

public class CatalogFilterItemViewModelTests
{
    [Fact]
    public void IsSelected_DefaultsFalse()
    {
        var sut = new CatalogFilterItemViewModel
        {
            Id = Guid.NewGuid(),
            Name = "Arabica"
        };

        sut.IsSelected.Should().BeFalse();
    }

    [Fact]
    public void IsSelected_TogglesCorrectly()
    {
        var sut = new CatalogFilterItemViewModel
        {
            Id = Guid.NewGuid(),
            Name = "Arabica"
        };

        sut.IsSelected = true;
        sut.IsSelected.Should().BeTrue();

        sut.IsSelected = false;
        sut.IsSelected.Should().BeFalse();
    }

    [Fact]
    public void IsSelected_RaisesPropertyChanged()
    {
        var sut = new CatalogFilterItemViewModel
        {
            Id = Guid.NewGuid(),
            Name = "Arabica"
        };
        var raised = false;
        sut.PropertyChanged += (_, e) => raised |= e.PropertyName == nameof(CatalogFilterItemViewModel.IsSelected);

        sut.IsSelected = true;

        raised.Should().BeTrue();
    }
}
