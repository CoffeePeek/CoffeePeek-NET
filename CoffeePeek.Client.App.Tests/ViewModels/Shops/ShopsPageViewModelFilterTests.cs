using System.Collections.ObjectModel;
using System.Net.Http;
using CoffeePeek.Client.App.Infrastructure.HTTP.Configuration;
using CoffeePeek.Client.App.Infrastructure.HTTP.Responses;
using CoffeePeek.Client.App.Infrastructure.HTTP.WebClients;
using CoffeePeek.Client.App.Services;
using CoffeePeek.Client.App.ViewModels.Shops;
using CoffeePeek.Client.App.ViewModels.Shops.Search;
using CoffeePeek.Contract.Dtos.Internal;
using CoffeePeek.Contract.Dtos.Shop;
using FluentAssertions;
using FluentResults;
using Moq;
using Xunit;

namespace CoffeePeek.Client.App.Tests.ViewModels.Shops;

public class ShopsPageViewModelFilterTests
{
    private readonly Mock<IWebCoffeeShopsClient> _shopsClientMock = new();
    private readonly Mock<IWebCatalogsClient> _catalogsClientMock = new();
    private readonly Mock<IWorkspaceShellNavigator> _navigatorMock = new();

    private ShopsPageViewModel CreateSut()
    {
        _shopsClientMock
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

        _catalogsClientMock
            .Setup(c => c.GetCitiesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(new GetCitiesResultDto { Cities = [] }));

        _catalogsClientMock
            .Setup(c => c.GetBeansAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(new GetBeansResultDto
            {
                Beans =
                [
                    new CoffeeBeansDto { Id = Guid.NewGuid(), Name = "Arabica" },
                    new CoffeeBeansDto { Id = Guid.NewGuid(), Name = "Robusta" }
                ]
            }));

        _catalogsClientMock
            .Setup(c => c.GetRoastersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(new GetRoastersResultDto
            {
                Roasters = [new RoasterDto { Id = Guid.NewGuid(), Name = "Local Roast" }]
            }));

        _catalogsClientMock
            .Setup(c => c.GetEquipmentAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(new GetEquipmentResultDto { Equipment = [] }));

        _catalogsClientMock
            .Setup(c => c.GetBrewMethodsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(new GetBrewMethodsResultDto
            {
                BrewMethods = [new BrewMethodDto { Id = Guid.NewGuid(), Name = "Pour Over" }]
            }));

        return new ShopsPageViewModel(
            _shopsClientMock.Object,
            _catalogsClientMock.Object,
            _navigatorMock.Object,
            new HttpClient(),
            new ApiOptions { BaseAddress = "https://localhost/" });
    }

    [Fact]
    public async Task InitializeAsync_LoadsCatalogFilterGroups()
    {
        var sut = CreateSut();

        await sut.InitializeAsync();

        sut.FilterGroups.Should().HaveCount(4);
        sut.FilterGroups[0].Items.Should().HaveCount(2);
        sut.FilterGroups[1].Items.Should().HaveCount(1);
        sut.FilterGroups[2].Items.Should().HaveCount(1);
        sut.FilterGroups[3].Items.Should().BeEmpty();
    }

    [Fact]
    public async Task ApplyFilters_ResetsPageAndReloadsShops()
    {
        var sut = CreateSut();
        await sut.InitializeAsync();

        sut.FilterGroups[0].Items[0].IsSelected = true;
        await sut.ApplyFiltersCommand.ExecuteAsync(null);

        sut.ActiveFilterCount.Should().Be(1);

        _shopsClientMock.Verify(c => c.SearchAsync(
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

        sut.FilterGroups[0].Items[0].IsSelected = true;
        sut.FilterGroups[0].Items[1].IsSelected = true;
        sut.FilterGroups[1].Items[0].IsSelected = true;

        await sut.ClearFiltersCommand.ExecuteAsync(null);

        sut.FilterGroups.SelectMany(g => g.Items).Should().AllSatisfy(f => f.IsSelected.Should().BeFalse());
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
        var id3 = Guid.NewGuid();
        var filters = new ObservableCollection<CatalogFilterItemViewModel>
        {
            new() { Id = id1, Name = "A", IsSelected = true },
            new() { Id = id2, Name = "B" },
            new() { Id = id3, Name = "C", IsSelected = true }
        };

        var result = ShopsPageViewModel.GetSelectedIds(filters);

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo([id1, id3]);
    }

    [Fact]
    public async Task LoadCatalogFilters_HandlesPartialFailure()
    {
        var sut = CreateSut();

        // Override successful setup applied inside CreateSut.
        _catalogsClientMock
            .Setup(c => c.GetBeansAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<GetBeansResultDto>("Network error"));

        await sut.InitializeAsync();

        sut.FilterGroups[0].Items.Should().BeEmpty();
        sut.FilterGroups[1].Items.Should().HaveCount(1);
        sut.FilterGroups[2].Items.Should().HaveCount(1);
    }
}
