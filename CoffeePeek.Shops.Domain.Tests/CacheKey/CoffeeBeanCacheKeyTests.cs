using CoffeePeek.Shared.Domain.Interfaces.Infrastructure;
using FluentAssertions;

namespace CoffeePeek.Shops.Domain.Tests.CacheKey;

public class CoffeeBeanCacheKeyTests
{
    [Fact]
    public void ListPattern_StartsWithSamePrefixAsListAllKey()
    {
        // Arrange
        var allKey = CoffeePeek.Shared.Domain.Interfaces.Infrastructure.CacheKey.CoffeeBean.ListAll().Key;
        var pattern = CoffeePeek.Shared.Domain.Interfaces.Infrastructure.CacheKey.CoffeeBean.ListPattern();
        var prefix = allKey.Split(':')[0];

        // Assert — ListPattern() must share the leading token with ListAll().Key
        // so Redis SCAN/KEYS-based cache invalidation actually matches the write key
        pattern.Should().StartWith(prefix);
        pattern.Should().EndWith("*");
    }
}
