using CoffeePeek.Account.Persistence.Configuration;
using CoffeePeek.MediaService.Data;
using CoffeePeek.Shops.Persistance.Configuration;
using CoffeeShop.Moderation.Persistence.Configuration;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CoffeePeek.Backend.Tests.Persistence;

public class MigrationSmokeTests
{
    public static TheoryData<Type> BackendDbContexts()
    {
        return
        [
            typeof(AccountDbContext),
            typeof(ShopsDbContext),
            typeof(ModerationDbContext),
            typeof(MediaDbContext)
        ];
    }

    [Theory]
    [MemberData(nameof(BackendDbContexts))]
    public void BackendDbContextAssemblies_ShouldContainMigrationsAndModelSnapshots(Type dbContextType)
    {
        dbContextType.Should().BeAssignableTo<DbContext>();

        var assemblyTypes = dbContextType.Assembly.GetTypes();
        var migrations = assemblyTypes
            .Where(type => type.IsAssignableTo(typeof(Migration)) && !type.IsAbstract)
            .ToArray();
        var snapshots = assemblyTypes
            .Where(type => type.IsAssignableTo(typeof(ModelSnapshot)) && !type.IsAbstract)
            .ToArray();

        migrations.Should().NotBeEmpty($"{dbContextType.Name} must have at least one EF migration");
        snapshots.Should().ContainSingle($"{dbContextType.Name} must have one EF model snapshot");
    }
}
