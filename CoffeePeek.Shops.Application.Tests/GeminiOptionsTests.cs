using CoffeePeek.Shared.Kernel.Options;
using FluentAssertions;

namespace CoffeePeek.Shops.Application.Tests;

public class GeminiOptionsTests
{
    [Fact]
    public void DefaultTimeout_IsLongEnoughForVisionParse()
    {
        new GeminiOptions().TimeoutSeconds.Should().BeGreaterThanOrEqualTo(90);
    }
}
