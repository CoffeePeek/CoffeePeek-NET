using Bogus;
using CoffeePeek.Contract.Dtos;
using CoffeePeek.Contract.Dtos.Schedule;
using CoffeePeek.Contract.Enums;
using CoffeePeek.Shops.Domain.Entities;
using CoffeePeek.Shops.Domain.Entities.CoffeeShopAggregate;
using CoffeePeek.Shops.Domain.Entities.ReviewAggregate;
using CoffeePeek.Shops.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeePeek.Shops.Infrastructure;

public static class ShopsDbInitializer
{
    private static readonly Guid AdminId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid TestUserId = Guid.Parse("00000000-0000-0000-0000-000000000002");

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ShopsDbContext>();

        await context.Database.MigrateAsync();

        if (await context.Shops.CountAsync() > 10) return;

        var faker = new Faker("ru");

        // 1. Справочники
        var cities = await EnsureCitiesAsync(context);
        var brewMethods = await EnsureBrewMethodsAsync(context);
        var coffeeBeans = await EnsureCoffeeBeansAsync(context);
        var roasters = await EnsureRoastersAsync(context);
        var equipments = await EnsureEquipmentsAsync(context);

        // 2. Кофейни
        var shops = new List<CoffeeShop>();
        for (var i = 0; i < 50; i++)
        {
            var shop = new CoffeeShop(AdminId, faker.Company.CompanyName() + " Coffee", faker.PickRandom<PriceRange>(), Guid.NewGuid());
            var city = faker.PickRandom(cities);
            
            // Координаты примерно вокруг Москвы или Питера
            var lat = city.Name == "Москва" ? 55.7558m + (decimal)faker.Random.Double(-0.1, 0.1) : 59.9343m + (decimal)faker.Random.Double(-0.1, 0.1);
            var lon = city.Name == "Москва" ? 37.6173m + (decimal)faker.Random.Double(-0.1, 0.1) : 30.3351m + (decimal)faker.Random.Double(-0.1, 0.1);
            
            shop.SetLocation(city.Id, faker.Address.StreetAddress(), lat, lon);
            shop.SetContact(faker.Internet.UserName(), faker.Internet.Email(), faker.Internet.Url(), faker.Phone.PhoneNumber());
            
            var schedules = new List<ShopSchedule>();
            for (int day = 0; day < 7; day++)
            {
                schedules.Add(ShopSchedule.Create((DayOfWeek)day, false, new List<ShopScheduleIntervalDto>
                {
                    new() { OpenTime = new TimeSpan(8, 0, 0), CloseTime = new TimeSpan(22, 0, 0) }
                }));
            }
            shop.AddSchedule(schedules);
            
            shop.SetBrewMethods(faker.PickRandom(brewMethods, faker.Random.Int(1, 3)));
            shop.SetBeans(faker.PickRandom(coffeeBeans, faker.Random.Int(1, 2)));
            shop.SetRoasters(faker.PickRandom(roasters, faker.Random.Int(1, 2)));

            shops.Add(shop);
        }

        context.Shops.AddRange(shops);
        await context.SaveChangesAsync();

        // 3. Отзывы и чекины
        // Используем TestUserId и немного AdminId
        var userIds = new[] { TestUserId, AdminId };
        
        foreach (var shop in shops)
        {
            var reviewCount = faker.Random.Int(0, 5);
            for (int j = 0; j < reviewCount; j++)
            {
                var userId = faker.PickRandom(userIds);
                var review = Review.Create(
                    shop.Id, 
                    userId, 
                    userId == TestUserId ? "coffee_lover" : "admin", 
                    faker.Lorem.Sentence(3), 
                    faker.Lorem.Paragraph(), 
                    new RatingDto { Place = faker.Random.Int(3, 5), Service = faker.Random.Int(3, 5), Coffee = faker.Random.Int(3, 5) }
                    );
                
                context.Reviews.Add(review);
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task<List<City>> EnsureCitiesAsync(ShopsDbContext context)
    {
        if (await context.Cities.AnyAsync()) return await context.Cities.ToListAsync();
        
        var cities = new List<City> { new("Москва"), new("Санкт-Петербург"), new("Екатеринбург"), new("Казань") };
        context.Cities.AddRange(cities);
        await context.SaveChangesAsync();
        return cities;
    }

    private static async Task<List<BrewMethod>> EnsureBrewMethodsAsync(ShopsDbContext context)
    {
        if (await context.BrewMethods.AnyAsync()) return await context.BrewMethods.ToListAsync();

        var methods = new[] { "Espresso", "V60", "Aeropress", "Chemex", "Batch Brew" }
            .Select(name => {
                var m = (BrewMethod)Activator.CreateInstance(typeof(BrewMethod), true)!;
                typeof(BrewMethod).GetProperty("Id")!.SetValue(m, Guid.NewGuid());
                typeof(BrewMethod).GetProperty("Name", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)!.SetValue(m, name);
                return m;
            }).ToList();

        context.BrewMethods.AddRange(methods);
        await context.SaveChangesAsync();
        return methods;
    }

    private static async Task<List<CoffeeBean>> EnsureCoffeeBeansAsync(ShopsDbContext context)
    {
        if (await context.CoffeeBeans.AnyAsync()) return await context.CoffeeBeans.ToListAsync();

        var beans = new[] { "Ethiopia Yirgacheffe", "Colombia Huila", "Brazil Santos", "Kenya AA" }
            .Select(name => {
                var b = (CoffeeBean)Activator.CreateInstance(typeof(CoffeeBean), true)!;
                typeof(CoffeeBean).GetProperty("Id")!.SetValue(b, Guid.NewGuid());
                typeof(CoffeeBean).GetProperty("Name", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)!.SetValue(b, name);
                return b;
            }).ToList();

        context.CoffeeBeans.AddRange(beans);
        await context.SaveChangesAsync();
        return beans;
    }

    private static async Task<List<Roaster>> EnsureRoastersAsync(ShopsDbContext context)
    {
        if (await context.Roasters.AnyAsync()) return await context.Roasters.ToListAsync();

        var roasters = new[] { "The Barn", "Five Senses", "Tasty Coffee", "Surf Coffee" }
            .Select(name => {
                var r = (Roaster)Activator.CreateInstance(typeof(Roaster), true)!;
                typeof(Roaster).GetProperty("Id")!.SetValue(r, Guid.NewGuid());
                typeof(Roaster).GetProperty("Name", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)!.SetValue(r, name);
                return r;
            }).ToList();

        context.Roasters.AddRange(roasters);
        await context.SaveChangesAsync();
        return roasters;
    }

    private static async Task<List<Equipment>> EnsureEquipmentsAsync(ShopsDbContext context)
    {
        if (await context.Equipments.AnyAsync()) return await context.Equipments.ToListAsync();

        var equipment = new[] { "La Marzocco Strada", "Mahlkönig EK43", "Victoria Arduino Eagle One", "Mythos One" }
            .Select(name => {
                var e = (Equipment)Activator.CreateInstance(typeof(Equipment), true)!;
                typeof(Equipment).GetProperty("Id")!.SetValue(e, Guid.NewGuid());
                typeof(Equipment).GetProperty("Name", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)!.SetValue(e, name);
                return e;
            }).ToList();

        context.Equipments.AddRange(equipment);
        await context.SaveChangesAsync();
        return equipment;
    }
}
