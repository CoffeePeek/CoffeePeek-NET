using System.Collections.ObjectModel;
using CoffeePeek.Client.App.Infrastructure.HTTP.Responses;
using CoffeePeek.Client.App.Infrastructure.HTTP.WebClients;
using CoffeePeek.Client.App.Services;
using CoffeePeek.Client.App.ViewModels.Shops;
using CoffeePeek.Client.App.ViewModels.Shops.Search;
using CoffeePeek.Contract.Dtos.Shop;
using FluentAssertions;
using FluentResults;
using Moq;
using Xunit;

namespace CoffeePeek.Client.App.Tests.ViewModels.Shops;

public class ShopsPageViewModelFilterTests
{
    private readonly Mock<IWebCoffeeShopsClient> _clientMock = new();
    private readonly Mock<IWorkspaceShellNavigator> _navigatorMock = new();

    private ShopsPageViewModel CreateSut()
    {
        _clientMock
            .Setup(c => c.SearchAsync(
                It.IsAny<string?>(), It.IsAny<Guid?>(),
                It.IsAny<Guid[]?>(), It.IsAny<Guid[]?>(),
                It.IsAny<Guid[]?>(), It.IsAny<Guid[]?>(),
                It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(new SearchShopsResultDto
            {
                CoffeeShops = [],
                TotalPages = 1,
                CurrentPage = 1,
                PageSize = 10,
                TotalItems = 0
            }));

        _clientMock
            .Setup(c => c.GetCitiesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(new GetCitiesResultDto { Cities = [] }));

        _clientMock
            .Setup(c => c.GetBeansAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(new GetBeansResultDto
            {
                Beans =
                [
                    new CoffeeBeansDto { Id = Guid.NewGuid(), Name = "Arabica" },
                    new CoffeeBeansDto { Id = Guid.NewGuid(), Name = "Robusta" }
                ]
            }));

        _clientMock
            .Setup(c => c.GetRoastersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(new GetRoastersResultDto
            {
                Roasters = [new RoasterDto { Id = Guid.NewGuid(), Name = "Local Roast" }]
            }));

        _clientMock
            .Setup(c => c.GetEquipmentAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(new GetEquipmentResultDto { Equipments = [] }));

        _clientMock
            .Setup(c => c.GetBrewMethodsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(new GetBrewMethodsResultDto
            {
                BrewMethods = [new BrewMethodDto { Id = Guid.NewGuid(), Name = "Pour Over" }]
            }));

        return new ShopsPageViewModel(_clientMock.Object, _navigatorMock.Object);
    }

    [Fact]
    public async Task InitializeAsync_LoadsCatalogFilters()
    {
        var sut = CreateSut();

        await sut.InitializeAsync();

        sut.BeanFilters.Should().HaveCount(2);
        sut.RoasterFilters.Should().HaveCount(1);
        sut.BrewMethodFilters.Should().HaveCount(1);
        sut.EquipmentFilters.Should().BeEmpty();
    }

    [Fact]
    public async Task ApplyFilters_ResetsPageAndReloadsShops()
    {
        var sut = CreateSut();
        await sut.InitializeAsync();

        sut.BeanFilters[0].IsSelected = true;
        sut.ApplyFiltersCommand.Execute(null);

        sut.ActiveFilterCount.Should().Be(1);

        _clientMock.Verify(c => c.SearchAsync(
            It.IsAny<string?>(), It.IsAny<Guid?>(),
            null,
            It.Is<Guid[]?>(ids => ids != null && ids.Length == 1),
            null, null,
            1, It.IsAny<int>(),
            It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ClearFilters_DeselectsAllAndReloads()
    {
        var sut = CreateSut();
        await sut.InitializeAsync();

        sut.BeanFilters[0].IsSelected = true;
        sut.BeanFilters[1].IsSelected = true;
        sut.RoasterFilters[0].IsSelected = true;

        sut.ClearFiltersCommand.Execute(null);

        sut.BeanFilters.Should().AllSatisfy(f => f.IsSelected.Should().BeFalse());
        sut.RoasterFilters.Should().AllSatisfy(f => f.IsSelected.Should().BeFalse());
        sut.ActiveFilterCount.Should().Be(0);
    }

    [Fact]
    public void GetSelectedIds_ReturnsNull_WhenNoneSelected()
    {
        var filters = new ObservableCollection<CatalogFilterItemViewModel>
        {
            new() { Id = Guid.NewGuid(), Name = "A" },
            new() { Id = Guid.NewGuid(), Name = "B" }
        };

        ShopsPageViewModel.GetSelectedIds(filters).Should().BeNull();
    }

    [Fact]
    public void GetSelectedIds_ReturnsSelectedIds_WhenSomeSelected()
    {
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var filters = new ObservableCollection<CatalogFilterItemViewModel>
        {
            new() { Id = id1, Name = "A", IsSelected = true },
            new() { Id = id2, Name = "B" },
            new() { Id = Guid.NewGuid(), Name = "C", IsSelected = true }
        };

        var result = ShopsPageViewModel.GetSelectedIds(filters);

        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(id1);
    }

    [Fact]
    public async Task LoadCatalogFilters_HandlesPartialFailure()
    {
        _clientMock
            .Setup(c => c.GetBeansAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<GetBeansResultDto>("Network error"));

        var sut = CreateSut();

        // Re-setup beans to fail
        _clientMock
            .Setup(c => c.GetBeansAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<GetBeansResultDto>("Network error"));

        await sut.InitializeAsync();

        sut.BeanFilters.Should().BeEmpty();
        sut.RoasterFilters.Should().HaveCount(1);
        sut.BrewMethodFilters.Should().HaveCount(1);
    }
}
