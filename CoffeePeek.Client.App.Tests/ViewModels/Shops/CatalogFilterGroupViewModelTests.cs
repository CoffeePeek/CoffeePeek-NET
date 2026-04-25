using CoffeePeek.Client.App.ViewModels.Shops.Search;
using FluentAssertions;
using Xunit;

namespace CoffeePeek.Client.App.Tests.ViewModels.Shops;

public class CatalogFilterGroupViewModelTests
{
    [Fact]
    public void ActiveFilterCount_UpdatesWhenItemSelectionChanges()
    {
        var sut = new CatalogFilterGroupViewModel("Beans");
        var item = new CatalogFilterItemViewModel { Id = Guid.NewGuid(), Name = "Arabica" };
        var raised = false;
        sut.PropertyChanged += (_, e) => raised |= e.PropertyName == nameof(CatalogFilterGroupViewModel.ActiveFilterCount);

        sut.ReplaceItems([item]);
        item.IsSelected = true;

        sut.ActiveFilterCount.Should().Be(1);
        raised.Should().BeTrue();
    }

    [Fact]
    public void ClearSelection_DeselectsAllItems()
    {
        var sut = new CatalogFilterGroupViewModel("Roasters");
        sut.ReplaceItems(
        [
            new CatalogFilterItemViewModel { Id = Guid.NewGuid(), Name = "A", IsSelected = true },
            new CatalogFilterItemViewModel { Id = Guid.NewGuid(), Name = "B", IsSelected = true }
        ]);

        sut.ClearSelection();

        sut.Items.Should().AllSatisfy(i => i.IsSelected.Should().BeFalse());
        sut.ActiveFilterCount.Should().Be(0);
    }
}
