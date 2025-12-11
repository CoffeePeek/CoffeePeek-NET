using CoffeePeek.Data.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CoffeePeek.Data.Tests.Repositories;

public class GenericRepositoryTests : IDisposable
{
    private readonly TestDbContext _dbContext;
    private readonly GenericRepository<TestEntity, TestDbContext> _sut;

    public GenericRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new TestDbContext(options);
        _sut = new GenericRepository<TestEntity, TestDbContext>(_dbContext);
    }

    [Fact]
    public async Task AddAsync_WithValidEntity_AddsEntityToDatabase()
    {
        // Arrange
        var entity = new TestEntity { Name = "Test" };

        // Act
        var result = await _sut.AddAsync(entity);
        await _dbContext.SaveChangesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        var savedEntity = await _dbContext.TestEntities.FirstOrDefaultAsync();
        savedEntity.Should().NotBeNull();
        savedEntity!.Name.Should().Be("Test");
    }

    [Fact]
    public async Task AddAsync_WithNullEntity_ThrowsArgumentNullException()
    {
        // Act & Assert
        await FluentActions.Invoking(() => _sut.AddAsync(null!))
            .Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ReturnsEntity()
    {
        // Arrange
        var entity = new TestEntity { Name = "Test" };
        _dbContext.TestEntities.Add(entity);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sut.GetByIdAsync(entity.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(entity.Id);
        result.Name.Should().Be("Test");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ReturnsNull()
    {
        // Act
        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_WithMultipleEntities_ReturnsAllEntities()
    {
        // Arrange
        _dbContext.TestEntities.AddRange(
            new TestEntity { Name = "Test1" },
            new TestEntity { Name = "Test2" },
            new TestEntity { Name = "Test3" }
        );
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(e => e.Name == "Test1");
        result.Should().Contain(e => e.Name == "Test2");
        result.Should().Contain(e => e.Name == "Test3");
    }

    [Fact]
    public async Task GetAllAsync_WithNoEntities_ReturnsEmptyArray()
    {
        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task FindAsync_WithMatchingPredicate_ReturnsMatchingEntities()
    {
        // Arrange
        _dbContext.TestEntities.AddRange(
            new TestEntity { Name = "Match1" },
            new TestEntity { Name = "NoMatch" },
            new TestEntity { Name = "Match2" }
        );
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sut.FindAsync(e => e.Name.StartsWith("Match"));

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(e => e.Name == "Match1");
        result.Should().Contain(e => e.Name == "Match2");
    }

    [Fact]
    public async Task FindAsync_WithNoMatches_ReturnsEmptyArray()
    {
        // Arrange
        _dbContext.TestEntities.Add(new TestEntity { Name = "Test" });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sut.FindAsync(e => e.Name == "NonExistent");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task FirstOrDefaultAsync_WithMatchingPredicate_ReturnsFirstMatch()
    {
        // Arrange
        _dbContext.TestEntities.AddRange(
            new TestEntity { Name = "Match1" },
            new TestEntity { Name = "Match2" }
        );
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sut.FirstOrDefaultAsync(e => e.Name.StartsWith("Match"));

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Match1");
    }

    [Fact]
    public async Task FirstOrDefaultAsync_WithNoMatch_ReturnsNull()
    {
        // Act
        var result = await _sut.FirstOrDefaultAsync(e => e.Name == "NonExistent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AnyAsync_WithMatchingPredicate_ReturnsTrue()
    {
        // Arrange
        _dbContext.TestEntities.Add(new TestEntity { Name = "Test" });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sut.AnyAsync(e => e.Name == "Test");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task AnyAsync_WithNoMatch_ReturnsFalse()
    {
        // Act
        var result = await _sut.AnyAsync(e => e.Name == "NonExistent");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CountAsync_WithoutPredicate_ReturnsTotal()
    {
        // Arrange
        _dbContext.TestEntities.AddRange(
            new TestEntity { Name = "Test1" },
            new TestEntity { Name = "Test2" },
            new TestEntity { Name = "Test3" }
        );
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sut.CountAsync();

        // Assert
        result.Should().Be(3);
    }

    [Fact]
    public async Task CountAsync_WithPredicate_ReturnsMatchingCount()
    {
        // Arrange
        _dbContext.TestEntities.AddRange(
            new TestEntity { Name = "Match" },
            new TestEntity { Name = "NoMatch" },
            new TestEntity { Name = "Match" }
        );
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sut.CountAsync(e => e.Name == "Match");

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public void Update_WithValidEntity_MarksEntityAsModified()
    {
        // Arrange
        var entity = new TestEntity { Name = "Original" };
        _dbContext.TestEntities.Add(entity);
        _dbContext.SaveChanges();
        entity.Name = "Updated";

        // Act
        _sut.Update(entity);
        _dbContext.SaveChanges();

        // Assert
        var updated = _dbContext.TestEntities.Find(entity.Id);
        updated!.Name.Should().Be("Updated");
    }

    [Fact]
    public void Update_WithNullEntity_ThrowsArgumentNullException()
    {
        // Act & Assert
        FluentActions.Invoking(() => _sut.Update(null!))
            .Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Remove_WithValidEntity_RemovesEntityFromDatabase()
    {
        // Arrange
        var entity = new TestEntity { Name = "ToDelete" };
        _dbContext.TestEntities.Add(entity);
        _dbContext.SaveChanges();

        // Act
        _sut.Remove(entity);
        _dbContext.SaveChanges();

        // Assert
        var deleted = _dbContext.TestEntities.Find(entity.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public void Remove_WithNullEntity_ThrowsArgumentNullException()
    {
        // Act & Assert
        FluentActions.Invoking(() => _sut.Remove(null!))
            .Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task AddRangeAsync_WithMultipleEntities_AddsAllEntities()
    {
        // Arrange
        var entities = new[]
        {
            new TestEntity { Name = "Test1" },
            new TestEntity { Name = "Test2" },
            new TestEntity { Name = "Test3" }
        };

        // Act
        await _sut.AddRangeAsync(entities);
        await _dbContext.SaveChangesAsync();

        // Assert
        var count = await _dbContext.TestEntities.CountAsync();
        count.Should().Be(3);
    }

    [Fact]
    public void Query_ReturnsQueryable()
    {
        // Arrange
        _dbContext.TestEntities.AddRange(
            new TestEntity { Name = "Test1" },
            new TestEntity { Name = "Test2" }
        );
        _dbContext.SaveChanges();

        // Act
        var query = _sut.Query();
        var result = query.Where(e => e.Name == "Test1").ToList();

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Test1");
    }

    [Fact]
    public void QueryAsNoTracking_ReturnsNonTrackedQueryable()
    {
        // Arrange
        _dbContext.TestEntities.Add(new TestEntity { Name = "Test" });
        _dbContext.SaveChanges();

        // Act
        var query = _sut.QueryAsNoTracking();
        var entity = query.First();
        entity.Name = "Modified";

        // Assert - changes should not be tracked
        var originalEntity = _dbContext.TestEntities.Find(entity.Id);
        originalEntity!.Name.Should().Be("Test");
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    // Test helper classes
    private class TestEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
    }

    private class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
        public DbSet<TestEntity> TestEntities { get; set; } = null!;
    }
}