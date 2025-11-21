using CoffeePeek.Contract.Constants;
using FluentAssertions;

namespace CoffeePeek.Contract.Tests.Constants;

public class BusinessConstantsTests
{
    [Fact]
    public void DefaultUnAuthorizedCityId_ShouldBeOne()
    {
        // Assert
        BusinessConstants.DefaultUnAuthorizedCityId.Should().Be(1);
    }

    [Fact]
    public void DefaultUnAuthorizedCityId_ShouldBeConstant()
    {
        // Arrange & Act
        var value1 = BusinessConstants.DefaultUnAuthorizedCityId;
        var value2 = BusinessConstants.DefaultUnAuthorizedCityId;

        // Assert
        value1.Should().Be(value2);
    }
}
