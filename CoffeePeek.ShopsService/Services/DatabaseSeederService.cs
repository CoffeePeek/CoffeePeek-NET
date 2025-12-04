using Bogus;
using CoffeePeek.Contract.Enums;
using CoffeePeek.ShopsService.DB;
using CoffeePeek.ShopsService.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.ShopsService.Services;

public class DatabaseSeederService
{
    private readonly ShopsDbContext _context;
    private readonly Faker _faker;

    public DatabaseSeederService(ShopsDbContext context)
    {
        _context = context;
        _faker = new Faker("ru");
    }

    public async Task SeedAsync()
    {
        // Проверяем, есть ли уже данные
        if (await _context.Cities.AnyAsync())
        {
            return; // База уже заполнена
        }

        // 1. Создаем города
        var cities = GenerateCities();
        await _context.Cities.AddRangeAsync(cities);
        await _context.SaveChangesAsync();

        // 2. Создаем методы заваривания
        var brewMethods = GenerateBrewMethods();
        await _context.BrewMethods.AddRangeAsync(brewMethods);
        await _context.SaveChangesAsync();

        // 3. Создаем виды кофе
        var coffeeBeans = GenerateCoffeeBeans();
        await _context.CoffeeBeans.AddRangeAsync(coffeeBeans);
        await _context.SaveChangesAsync();

        // 4. Создаем оборудование
        var equipments = GenerateEquipments();
        await _context.Equipments.AddRangeAsync(equipments);
        await _context.SaveChangesAsync();

        // 5. Создаем обжарщиков
        var roasters = GenerateRoasters();
        await _context.Roasters.AddRangeAsync(roasters);
        await _context.SaveChangesAsync();

        // 6. Создаем магазины с контактами, локациями, фото и расписанием
        var shops = await GenerateShopsAsync(cities);

        // 7. Создаем связи магазинов с методами заваривания
        var shopBrewMethods = GenerateShopBrewMethods(shops, brewMethods);
        await _context.ShopBrewMethods.AddRangeAsync(shopBrewMethods);
        await _context.SaveChangesAsync();

        // 8. Создаем связи магазинов с видами кофе
        var coffeeBeanShops = GenerateCoffeeBeanShops(shops, coffeeBeans);
        await _context.CoffeeBeanShops.AddRangeAsync(coffeeBeanShops);
        await _context.SaveChangesAsync();

        // 9. Создаем связи магазинов с оборудованием
        var shopEquipments = GenerateShopEquipments(shops, equipments);
        await _context.ShopEquipments.AddRangeAsync(shopEquipments);
        await _context.SaveChangesAsync();

        // 10. Создаем связи магазинов с обжарщиками
        var roasterShops = GenerateRoasterShops(shops, roasters);
        await _context.RoasterShops.AddRangeAsync(roasterShops);
        await _context.SaveChangesAsync();

        // 11. Создаем отзывы
        var reviews = GenerateReviews(shops);
        await _context.Reviews.AddRangeAsync(reviews);
        await _context.SaveChangesAsync();
    }

    private List<City> GenerateCities()
    {
        var cityNames = new[]
        {
            "Москва", "Санкт-Петербург", "Новосибирск", "Екатеринбург", "Казань",
            "Нижний Новгород", "Челябинск", "Самара", "Омск", "Ростов-на-Дону"
        };

        return cityNames.Select(name => new City
        {
            Id = Guid.NewGuid(),
            Name = name
        }).ToList();
    }

    private List<BrewMethod> GenerateBrewMethods()
    {
        var methodNames = new[]
        {
            "Эспрессо", "Американо", "Капучино", "Латте", "Флэт Уайт",
            "Френч Пресс", "Вьетнамский фильтр", "Кемекс", "Аэропресс", "Холодное заваривание"
        };

        return methodNames.Select(name => new BrewMethod
        {
            Id = Guid.NewGuid(),
            Name = name
        }).ToList();
    }

    private List<CoffeeBean> GenerateCoffeeBeans()
    {
        var beanNames = new[]
        {
            "Арабика", "Робуста", "Либерика", "Эксцельса", "Марагоджип",
            "Типика", "Бурбон", "Катурра", "Мундо Ново", "Катуаи"
        };

        return beanNames.Select(name => new CoffeeBean
        {
            Id = Guid.NewGuid(),
            Name = name
        }).ToList();
    }

    private List<Equipment> GenerateEquipments()
    {
        var equipmentNames = new[]
        {
            "Эспрессо-машина", "Кофемолка", "Френч-пресс", "Кемекс", "Аэропресс",
            "Вьетнамский фильтр", "Холодная заварка", "Турка", "Мока-пот", "Сифон"
        };

        return equipmentNames.Select(name => new Equipment
        {
            Id = Guid.NewGuid(),
            Name = name
        }).ToList();
    }

    private List<Roaster> GenerateRoasters()
    {
        var roasterNames = new[]
        {
            "Кофейная компания", "Обжарщик №1", "Кофе-мастер", "Ростер-про", "Кофейная плантация",
            "Элитный ростер", "Кофе-эксперт", "Обжарка премиум", "Кофейный дом", "Ростер-классик"
        };

        return roasterNames.Select(name => new Roaster
        {
            Id = Guid.NewGuid(),
            Name = name
        }).ToList();
    }

    private async Task<List<Shop>> GenerateShopsAsync(List<City> cities)
    {
        var shopFaker = new Faker<Shop>("ru")
            .RuleFor(s => s.Id, f => Guid.NewGuid())
            .RuleFor(s => s.Name, f => $"{f.Company.CompanyName()} Coffee")
            .RuleFor(s => s.Description, f => f.Lorem.Paragraph())
            .RuleFor(s => s.PriceRange, f => f.PickRandom<PriceRange>())
            .RuleFor(s => s.CityId, f => f.PickRandom(cities).Id)
            .RuleFor(s => s.ShopContactId, _ => (Guid?)null)
            .RuleFor(s => s.LocationId, _ => (Guid?)null);

        var shops = shopFaker.Generate(50);
        var shopContacts = new List<ShopContact>();
        var locations = new List<Location>();
        var shopPhotos = new List<ShopPhoto>();
        var shopSchedules = new List<ShopSchedule>();
        var scheduleIntervals = new List<ShopScheduleInterval>();

        foreach (var shop in shops)
        {
            // Создаем контакт
            var contact = new ShopContact
            {
                Id = Guid.NewGuid(),
                ShopId = shop.Id,
                Email = _faker.Internet.Email(),
                InstagramLink = $"https://instagram.com/{_faker.Internet.UserName()}",
                SiteLink = _faker.Internet.Url(),
                PhoneNumber = _faker.Phone.PhoneNumber("+7 (###) ###-##-##")
            };
            shopContacts.Add(contact);
            shop.ShopContactId = contact.Id;

            // Создаем локацию
            var location = new Location
            {
                Id = Guid.NewGuid(),
                ShopId = shop.Id,
                Address = _faker.Address.StreetAddress(),
                Latitude = (decimal)_faker.Address.Latitude(55.0, 56.0), // Пример для Москвы
                Longitude = (decimal)_faker.Address.Longitude(37.0, 38.0)
            };
            locations.Add(location);
            shop.LocationId = location.Id;

            // Создаем фото для магазина (2-5 фото)
            var photoCount = _faker.Random.Int(2, 5);
            for (var i = 0; i < photoCount; i++)
            {
                shopPhotos.Add(new ShopPhoto
                {
                    Id = Guid.NewGuid(),
                    ShopId = shop.Id,
                    UserId = Guid.NewGuid(),
                    Url = _faker.Image.PicsumUrl(800, 600),
                    CreatedAt = _faker.Date.Past(1).ToUniversalTime(),
                    UpdatedAt = DateTime.UtcNow
                });
            }

            // Создаем расписание для магазина (на каждый день недели)
            var daysOfWeek = Enum.GetValues<DayOfWeek>();
            foreach (var day in daysOfWeek)
            {
                var isClosed = _faker.Random.Bool(0.1f); // 10% вероятность закрытия
                var schedule = new ShopSchedule
                {
                    Id = Guid.NewGuid(),
                    ShopId = shop.Id,
                    DayOfWeek = day,
                    IsClosed = isClosed,
                    Intervals = new List<ShopScheduleInterval>()
                };

                if (!isClosed)
                {
                    // Создаем интервалы работы (обычно 1-2 интервала в день)
                    var intervalCount = _faker.Random.Int(1, 2);
                    for (var i = 0; i < intervalCount; i++)
                    {
                        var openHour = _faker.Random.Int(7, 10);
                        var closeHour = _faker.Random.Int(18, 23);
                        var openMinute = _faker.Random.Int(0, 59);
                        var closeMinute = _faker.Random.Int(0, 59);

                        var interval = new ShopScheduleInterval
                        {
                            Id = Guid.NewGuid(),
                            ScheduleId = schedule.Id,
                            OpenTime = new TimeSpan(openHour, openMinute, 0),
                            CloseTime = new TimeSpan(closeHour, closeMinute, 0)
                        };
                        scheduleIntervals.Add(interval);
                        schedule.Intervals.Add(interval);
                    }
                }

                shopSchedules.Add(schedule);
            }
        }

        // Сохраняем все связанные данные
        await _context.ShopContacts.AddRangeAsync(shopContacts);
        await _context.Locations.AddRangeAsync(locations);
        await _context.ShopPhotos.AddRangeAsync(shopPhotos);
        await _context.ShopSchedules.AddRangeAsync(shopSchedules);
        await _context.ShopScheduleIntervals.AddRangeAsync(scheduleIntervals);
        await _context.Shops.AddRangeAsync(shops);
        await _context.SaveChangesAsync();

        return shops;
    }

    private List<ShopBrewMethod> GenerateShopBrewMethods(List<Shop> shops, List<BrewMethod> brewMethods)
    {
        var shopBrewMethods = new List<ShopBrewMethod>();

        foreach (var shop in shops)
        {
            // Каждый магазин использует 3-7 методов заваривания
            var methodCount = _faker.Random.Int(3, 7);
            var selectedMethods = _faker.PickRandom(brewMethods, methodCount);

            foreach (var method in selectedMethods)
            {
                shopBrewMethods.Add(new ShopBrewMethod
                {
                    Id = Guid.NewGuid(),
                    ShopId = shop.Id,
                    BrewMethodId = method.Id
                });
            }
        }

        return shopBrewMethods;
    }

    private List<CoffeeBeanShop> GenerateCoffeeBeanShops(List<Shop> shops, List<CoffeeBean> coffeeBeans)
    {
        var coffeeBeanShops = new List<CoffeeBeanShop>();

        foreach (var shop in shops)
        {
            // Каждый магазин предлагает 2-5 видов кофе
            var beanCount = _faker.Random.Int(2, 5);
            var selectedBeans = _faker.PickRandom(coffeeBeans, beanCount);

            foreach (var bean in selectedBeans)
            {
                coffeeBeanShops.Add(new CoffeeBeanShop
                {
                    Id = Guid.NewGuid(),
                    ShopId = shop.Id,
                    CoffeeBeanId = bean.Id
                });
            }
        }

        return coffeeBeanShops;
    }

    private List<ShopEquipment> GenerateShopEquipments(List<Shop> shops, List<Equipment> equipments)
    {
        var shopEquipments = new List<ShopEquipment>();

        foreach (var shop in shops)
        {
            // Каждый магазин имеет 3-8 единиц оборудования
            var equipmentCount = _faker.Random.Int(3, 8);
            var selectedEquipments = _faker.PickRandom(equipments, equipmentCount);

            foreach (var equipment in selectedEquipments)
            {
                shopEquipments.Add(new ShopEquipment
                {
                    Id = Guid.NewGuid(),
                    ShopId = shop.Id,
                    EquipmentId = equipment.Id
                });
            }
        }

        return shopEquipments;
    }

    private List<RoasterShop> GenerateRoasterShops(List<Shop> shops, List<Roaster> roasters)
    {
        var roasterShops = new List<RoasterShop>();

        foreach (var shop in shops)
        {
            // Каждый магазин работает с 1-3 обжарщиками
            var roasterCount = _faker.Random.Int(1, 3);
            var selectedRoasters = _faker.PickRandom(roasters, roasterCount);

            foreach (var roaster in selectedRoasters)
            {
                roasterShops.Add(new RoasterShop
                {
                    Id = Guid.NewGuid(),
                    ShopId = shop.Id,
                    RoasterId = roaster.Id
                });
            }
        }

        return roasterShops;
    }

    private List<Review> GenerateReviews(List<Shop> shops)
    {
        var reviews = new List<Review>();

        foreach (var shop in shops)
        {
            // Каждый магазин имеет 5-20 отзывов
            var reviewCount = _faker.Random.Int(5, 20);

            for (var i = 0; i < reviewCount; i++)
            {
                var reviewDate = _faker.Date.Past(1).ToUniversalTime();
                reviews.Add(new Review
                {
                    Id = Guid.NewGuid(),
                    ShopId = shop.Id,
                    UserId = Guid.NewGuid(),
                    Header = _faker.Lorem.Sentence(5, 10),
                    Comment = _faker.Lorem.Paragraph(),
                    ReviewDate = reviewDate,
                    CreatedAt = reviewDate,
                    RatingCoffee = _faker.Random.Decimal(1, 5),
                    RatingPlace = _faker.Random.Decimal(1, 5),
                    RatingService = _faker.Random.Decimal(1, 5)
                });
            }
        }

        return reviews;
    }
}

