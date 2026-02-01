using CoffeePeek.Account.Domain.Entities.RoleAggregate;
using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Auth.Infrastructure.Configuration;
using CoffeePeek.Shared.Infrastructure.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Bogus;

namespace CoffeePeek.Auth.Infrastructure;

public static class AccountDbInitializer
{
    public static readonly Guid AdminId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    public static readonly Guid TestUserId = Guid.Parse("00000000-0000-0000-0000-000000000002");

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AccountDbContext>();

        await context.Database.MigrateAsync();

        if (!await context.Roles.AnyAsync())
        {
            var roles = new List<Role>
            {
                Role.Create(RoleConsts.Admin),
                Role.Create(RoleConsts.Moderator),
                Role.Create(RoleConsts.User),
                Role.Create(RoleConsts.Owner),
                Role.Create(RoleConsts.Employee),
                Role.Create(RoleConsts.Roaster)
            };

            context.Roles.AddRange(roles);
            await context.SaveChangesAsync();
        }

        if (await context.Users.CountAsync() <= 2)
        {
            var roles = await context.Roles.ToListAsync();
            var adminRole = roles.First(r => r.Name == RoleConsts.Admin);
            var userRole = roles.First(r => r.Name == RoleConsts.User);

            if (!await context.Users.AnyAsync(u => u.Id == AdminId))
            {
                var admin = User.Register("admin@coffeepeek.com", "admin", "$2a$11$q.vR2X1P7Pz8J8A8/8z8U.A.A.A.A.A.A.A.A.A.A.A.A.A.A.A.", adminRole);
                typeof(User).GetProperty("Id")?.SetValue(admin, AdminId);
                admin.ConfirmEmail(admin.Credentials.EmailConfirmationToken!);
                context.Users.Add(admin);
            }

            if (!await context.Users.AnyAsync(u => u.Id == TestUserId))
            {
                var user = User.Register("user@coffeepeek.com", "coffee_lover", "$2a$11$q.vR2X1P7Pz8J8A8/8z8U.A.A.A.A.A.A.A.A.A.A.A.A.A.A.A.", userRole);
                typeof(User).GetProperty("Id")?.SetValue(user, TestUserId);
                user.ConfirmEmail(user.Credentials.EmailConfirmationToken!);
                context.Users.Add(user);
            }

            // Генерируем много пользователей
            var faker = new Faker("ru");
            var passwordHash = "$2a$11$q.vR2X1P7Pz8J8A8/8z8U.A.A.A.A.A.A.A.A.A.A.A.A.A.A.A.";
            
            var usersToCreate = 100;
            var newUsers = new List<User>();

            for (int i = 0; i < usersToCreate; i++)
            {
                var email = faker.Internet.Email();
                var username = faker.Internet.UserName();
                var role = faker.PickRandom(roles);

                try 
                {
                    var user = User.Register(email, username, passwordHash, role);
                    user.ConfirmEmail(user.Credentials.EmailConfirmationToken!);
                    
                    if (faker.Random.Bool(0.3f))
                    {
                        user.UpdateAbout(faker.Lorem.Sentence());
                    }
                    
                    newUsers.Add(user);
                }
                catch
                {
                    // Игнорируем ошибки валидации Bogus данных (редко, но бывает невалидный email/username для нашей логики)
                    i--;
                }
            }

            context.Users.AddRange(newUsers);
            await context.SaveChangesAsync();
        }
    }
}
