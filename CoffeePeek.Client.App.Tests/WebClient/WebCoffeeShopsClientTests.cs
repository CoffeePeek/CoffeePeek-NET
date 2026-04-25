using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Models;
using CoffeePeek.Client.App.Infrastructure.WebClient;
using FluentAssertions;
using Xunit;

namespace CoffeePeek.Client.App.Tests.WebClient;

public class WebCoffeeShopsClientTests
{
    [Fact]
    public void AppendGuidArray_Null_DoesNotModifyQuery()
    {
        var command = new HttpCommand();

        WebCoffeeShopsClient.AppendGuidArray(command, "roasters", null);

        command.Query.Should().BeEmpty();
    }

    [Fact]
    public void AppendGuidArray_Empty_DoesNotModifyQuery()
    {
        var command = new HttpCommand();

        WebCoffeeShopsClient.AppendGuidArray(command, "beans", []);

        command.Query.Should().BeEmpty();
    }

    [Fact]
    public void AppendGuidArray_SingleId_AddsIndexedKey()
    {
        var command = new HttpCommand();
        var id = Guid.NewGuid();

        WebCoffeeShopsClient.AppendGuidArray(command, "roasters", [id]);

        command.Query.Should().ContainKey("roasters[0]");
        command.Query["roasters[0]"].Should().Be(id.ToString("D"));
    }

    [Fact]
    public void AppendGuidArray_MultipleIds_AddsIndexedKeys()
    {
        var command = new HttpCommand();
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var id3 = Guid.NewGuid();

        WebCoffeeShopsClient.AppendGuidArray(command, "beans", [id1, id2, id3]);

        command.Query.Should().HaveCount(3);
        command.Query["beans[0]"].Should().Be(id1.ToString("D"));
        command.Query["beans[1]"].Should().Be(id2.ToString("D"));
        command.Query["beans[2]"].Should().Be(id3.ToString("D"));
    }

    [Fact]
    public void AppendGuidArray_MultipleDifferentKeys_DoNotConflict()
    {
        var command = new HttpCommand();
        var roasterId = Guid.NewGuid();
        var beanId = Guid.NewGuid();

        WebCoffeeShopsClient.AppendGuidArray(command, "roasters", [roasterId]);
        WebCoffeeShopsClient.AppendGuidArray(command, "beans", [beanId]);

        command.Query.Should().HaveCount(2);
        command.Query["roasters[0]"].Should().Be(roasterId.ToString("D"));
        command.Query["beans[0]"].Should().Be(beanId.ToString("D"));
    }

    [Fact]
    public void AppendGuidArray_CallTwiceWithSameKey_OverwritesSameIndexedKeys()
    {
        var command = new HttpCommand();
        var firstId = Guid.NewGuid();
        var secondId = Guid.NewGuid();

        WebCoffeeShopsClient.AppendGuidArray(command, "roasters", [firstId]);
        WebCoffeeShopsClient.AppendGuidArray(command, "roasters", [secondId]);

        command.Query.Should().ContainSingle();
        command.Query["roasters[0]"].Should().Be(secondId.ToString("D"));
    }
}
