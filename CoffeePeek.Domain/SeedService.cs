using Bogus;
using CoffeePeek.Domain.Databases;
using CoffeePeek.Domain.Entities.Address;
using CoffeePeek.Domain.Entities.Schedules;
using CoffeePeek.Domain.Entities.Shop;
using CoffeePeek.Domain.Enums.Shop;
using City = CoffeePeek.Domain.Entities.Address.City;
using Country = CoffeePeek.Domain.Entities.Address.Country;

namespace CoffeePeek.Domain;

public static class SeedService
{
    public static async Task SeedShopsAsync(CoffeePeekDbContext context)
    {
        if (context.Shops.Count() > 500)
            return;

        var random = new Random();

        // --- Общие статические данные ---
        var photoUrls = new[]
        {
            "https://images.unsplash.com/photo-1638882267964-0d9764607947?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHxtb2Rlcm4lMjBjb2ZmZWUlMjBjYWZlfGVufDF8fHx8MTc2NDY5NDg3M3ww&ixlib=rb-4.1.0&q=80&w=1080",
            "https://images.unsplash.com/photo-1712942851408-6deb69dc4185?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHxjb3p5JTIwY29mZmVlJTIwaG91c2V8ZW58MXx8fHwxNzY0Njk0ODczfDA&ixlib=rb-4.1.0&q=80&w=1080",
            "https://images.unsplash.com/photo-1635236796520-68dd8df87895?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHxtaW5pbWFsaXN0JTIwY2FmZXxlbnwxfHx8fDE3NjQ2OTQ4NzN8MA&ixlib=rb-4.1.0&q=80&w=1080"
        };

        // --- Country ---
        var countryFaker = new Faker<Country>("ru")
            .RuleFor(c => c.Name, f => f.Address.Country());

        var countries = countryFaker.Generate(3);
        await context.Countries.AddRangeAsync(countries);
        await context.SaveChangesAsync();

        // --- City ---
        var cityFaker = new Faker<City>("ru")
            .RuleFor(c => c.Name, f => f.Address.City())
            .RuleFor(c => c.CountryId, f => f.PickRandom(countries).Id);

        var cities = cityFaker.Generate(10);
        await context.Cities.AddRangeAsync(cities);
        await context.SaveChangesAsync();

        // --- Street ---
        var streetFaker = new Faker<Street>("ru")
            .RuleFor(s => s.Name, f => f.Address.StreetName())
            .RuleFor(s => s.CityId, f => f.PickRandom(cities).Id);

        var streets = streetFaker.Generate(20);
        await context.Streets.AddRangeAsync(streets);
        await context.SaveChangesAsync();

        // --- Addresses ---
        var addressFaker = new Faker<Address>("ru")
            .RuleFor(a => a.CityId, f => f.PickRandom(cities).Id)
            .RuleFor(a => a.StreetId, f => f.PickRandom(streets).Id)
            .RuleFor(a => a.BuildingNumber, f => f.Address.BuildingNumber())
            .RuleFor(a => a.PostalCode, f => f.Address.ZipCode())
            .RuleFor(a => a.Latitude, f => decimal.Parse(f.Address.Latitude().ToString("0.000000")))
            .RuleFor(a => a.Longitude, f => decimal.Parse(f.Address.Longitude().ToString("0.000000")));

        var addresses = addressFaker.Generate(20);
        await context.Addresses.AddRangeAsync(addresses);
        await context.SaveChangesAsync();

        // --- Shops ---
        var shopFaker = new Faker<Shop>("ru")
            .RuleFor(s => s.Name, f => f.Company.CompanyName())
            .RuleFor(s => s.AddressId, f => f.PickRandom(addresses).Id)
            .RuleFor(s => s.Status, f => f.PickRandom<ShopStatus>())
            .RuleFor(s => s.ShopContactId, f => null);

        var shops = shopFaker.Generate(150);
        await context.Shops.AddRangeAsync(shops);
        await context.SaveChangesAsync();

        // --- Shop Contacts ---
        var contactsFaker = new Faker<ShopContacts>("ru")
            .RuleFor(c => c.ShopId, f => f.PickRandom(shops).Id)
            .RuleFor(c => c.PhoneNumber, f => f.Phone.PhoneNumber("+3706#######"))
            .RuleFor(c => c.InstagramLink, f => $"https://instagram.com/{f.Internet.UserName()}");

        var contacts = contactsFaker.Generate(10);
        await context.ShopContacts.AddRangeAsync(contacts);
        await context.SaveChangesAsync();

        // Устанавливаем ShopContactId
        foreach (var c in contacts)
        {
            var shop = shops.First(s => s.Id == c.ShopId);
            shop.ShopContactId = c.Id;
        }

        await context.SaveChangesAsync();

        // --- Photos ---
        var photoFaker = new Faker<ShopPhoto>("ru")
            .RuleFor(p => p.ShopId, f => f.PickRandom(shops).Id)
            .RuleFor(p => p.Url, f => f.PickRandom(photoUrls));

        var photos = photoFaker.Generate(30);
        await context.ShopPhotos.AddRangeAsync(photos);
        await context.SaveChangesAsync();

        // --- Schedules ---
        var schedules = new List<Schedule>();
        foreach (var shop in shops)
        {
            for (int i = 0; i < 7; i++)
            {
                schedules.Add(new Schedule
                {
                    ShopId = shop.Id,
                    DayOfWeek = (DayOfWeek)i,
                    OpeningTime = new TimeSpan(8, 0, 0),
                    ClosingTime = new TimeSpan(20, 0, 0)
                });
            }
        }

        await context.Schedules.AddRangeAsync(schedules);
        await context.SaveChangesAsync();

        // --- Random Schedule Exceptions ---
        var exceptionFaker = new Faker<ScheduleException>("ru")
            .RuleFor(e => e.ShopId, f => f.PickRandom(shops).Id)
            .RuleFor(e => e.ExceptionStartDate, f => f.Date.FutureOffset(1).Date)
            .RuleFor(e => e.ExceptionEndDate, (f, e) => e.ExceptionStartDate.AddDays(f.Random.Int(1, 3)))
            .RuleFor(e => e.SpecialOpeningTime, f => new DateTime(10, 0, 0))
            .RuleFor(e => e.SpecialClosingTime, f => new DateTime(18, 0, 0))
            .RuleFor(e => e.IsSpecialOpen24Hours, f => f.Random.Bool(0.1f));

        var exceptions = exceptionFaker.Generate(20);
        await context.ScheduleExceptions.AddRangeAsync(exceptions);
        await context.SaveChangesAsync();
    }
}