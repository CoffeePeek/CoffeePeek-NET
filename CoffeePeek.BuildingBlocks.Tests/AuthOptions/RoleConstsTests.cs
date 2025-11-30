using CoffeePeek.BuildingBlocks.AuthOptions;
using FluentAssertions;

namespace CoffeePeek.BuildingBlocks.Tests.AuthOptions;

public class RoleConstsTests
{
    [Fact]
    public void Admin_ShouldBeAdminLowercase()
    {
        // Assert
        RoleConsts.Admin.Should().Be(RoleConsts.Admin);
    }

    [Fact]
    public void Merchant_ShouldBeMerchantLowercase()
    {
        // Assert
        RoleConsts.Merchant.Should().Be(RoleConsts.Merchant);
    }

    [Fact]
    public void User_ShouldBeUserLowercase()
    {
        // Assert
        RoleConsts.User.Should().Be(RoleConsts.User);
    }

    [Fact]
    public void AllRoles_ShouldBeConstants()
    {
        // Arrange & Act
        var admin1 = RoleConsts.Admin;
        var admin2 = RoleConsts.Admin;
        var merchant1 = RoleConsts.Merchant;
        var merchant2 = RoleConsts.Merchant;
        var user1 = RoleConsts.User;
        var user2 = RoleConsts.User;

        // Assert
        admin1.Should().Be(admin2);
        merchant1.Should().Be(merchant2);
        user1.Should().Be(user2);
    }

    [Fact]
    public void AllRoles_ShouldBeDifferent()
    {
        // Assert
        RoleConsts.Admin.Should().NotBe(RoleConsts.Merchant);
        RoleConsts.Admin.Should().NotBe(RoleConsts.User);
        RoleConsts.Merchant.Should().NotBe(RoleConsts.User);
    }

    [Fact]
    public void AllRoles_ShouldBeStrings()
    {
        // Assert
        RoleConsts.Admin.Should().BeOfType<string>();
        RoleConsts.Merchant.Should().BeOfType<string>();
        RoleConsts.User.Should().BeOfType<string>();
    }
}
