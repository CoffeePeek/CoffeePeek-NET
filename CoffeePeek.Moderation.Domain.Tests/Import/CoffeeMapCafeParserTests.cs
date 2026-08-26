using CoffeePeek.Moderation.Domain.Aggregates.ShopImportCandidateAggregate;
using CoffeePeek.Moderation.Domain.Import;
using FluentAssertions;

namespace CoffeePeek.Moderation.Domain.Tests.Import;

public class CoffeeMapCafeParserTests
{
    [Fact]
    public void Parse_Array_MapsHoursAndInstagramFromWebsite()
    {
        const string json = """
            [
              {
                "id": "42",
                "name": "Lavazza Club Coffee",
                "address": "Немига 5, Минск",
                "lat": 53.9,
                "lng": 27.56,
                "website": "https://instagram.com/lavazza.minsk",
                "is_specialty": true,
                "recommended": false,
                "hours": {
                  "mon": { "open": "10:00", "close": "22:00" },
                  "sun": { "open": "11:00", "close": "20:00" }
                },
                "has_wifi": true
              }
            ]
            """;

        var cafes = CoffeeMapCafeParser.Parse(json);
        cafes.Should().ContainSingle();
        var cafe = cafes[0];
        cafe.ExternalId.Should().Be("42");
        cafe.Instagram.Should().Be("https://instagram.com/lavazza.minsk");
        cafe.OpeningHours.Should().Be("Mo 10:00-22:00; Su 11:00-20:00");
        cafe.IsSpecialty.Should().BeTrue();
        cafe.AmenitySignals.Should().Contain("coffeemap:wifi");
    }

    [Fact]
    public void Parse_WrappedCafes_SkipsDuplicatesAndMissingCoords()
    {
        const string json = """
            {
              "cafes": [
                { "id": "1", "name": "A", "lat": 53.9, "lng": 27.5 },
                { "id": "1", "name": "A dup", "lat": 53.9, "lng": 27.5 },
                { "id": "2", "name": "No coords" }
              ]
            }
            """;

        CoffeeMapCafeParser.Parse(json).Select(c => c.ExternalId).Should().Equal("1");
    }
}
