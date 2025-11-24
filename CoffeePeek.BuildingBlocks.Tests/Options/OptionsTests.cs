using CoffeePeek.BuildingBlocks.EfCore;
using FluentAssertions;

namespace CoffeePeek.BuildingBlocks.Tests.Options;

public class RabbitMqOptionsTests
{
    [Fact]
    public void RabbitMqOptions_DefaultConstructor_ShouldInitializeProperties()
    {
        // Act
        var options = new RabbitMqOptions();

        // Assert
        options.Should().NotBeNull();
        options.HostName.Should().BeNull();
        options.Username.Should().BeNull();
        options.Password.Should().BeNull();
        options.Port.Should().Be(0);
    }

    [Fact]
    public void RabbitMqOptions_Properties_ShouldBeSettable()
    {
        // Arrange
        var options = new RabbitMqOptions();

        // Act
        options.HostName = "localhost";
        options.Username = "guest";
        options.Password = "guest";
        options.Port = 5672;

        // Assert
        options.HostName.Should().Be("localhost");
        options.Username.Should().Be("guest");
        options.Password.Should().Be("guest");
        options.Port.Should().Be(5672);
    }

    [Theory]
    [InlineData("localhost", 5672)]
    [InlineData("rabbitmq.example.com", 5672)]
    [InlineData("192.168.1.100", 5673)]
    public void RabbitMqOptions_ShouldHandleVariousHostsAndPorts(string hostName, ushort port)
    {
        // Arrange & Act
        var options = new RabbitMqOptions
        {
            HostName = hostName,
            Port = port
        };

        // Assert
        options.HostName.Should().Be(hostName);
        options.Port.Should().Be(port);
    }

    [Fact]
    public void RabbitMqOptions_Port_ShouldBeUnsignedShort()
    {
        // Arrange & Act
        var options = new RabbitMqOptions
        {
            Port = ushort.MaxValue
        };

        // Assert
        options.Port.Should().Be(ushort.MaxValue);
        options.Port.Should().BeOfType(typeof(ushort));
    }

    [Fact]
    public void RabbitMqOptions_ShouldAllowEmptyCredentials()
    {
        // Arrange & Act
        var options = new RabbitMqOptions
        {
            Username = "",
            Password = ""
        };

        // Assert
        options.Username.Should().BeEmpty();
        options.Password.Should().BeEmpty();
    }
}

public class RedisOptionsTests
{
    [Fact]
    public void RedisOptions_DefaultConstructor_ShouldInitializeProperties()
    {
        // Act
        var options = new RedisOptions.RedisOptions();

        // Assert
        options.Should().NotBeNull();
        options.Host.Should().BeNull();
        options.Port.Should().Be(0);
        options.Password.Should().BeNull();
    }

    [Fact]
    public void RedisOptions_Properties_ShouldBeSettable()
    {
        // Arrange
        var options = new RedisOptions.RedisOptions();

        // Act
        options.Host = "localhost";
        options.Port = 6379;
        options.Password = "redis-password";

        // Assert
        options.Host.Should().Be("localhost");
        options.Port.Should().Be(6379);
        options.Password.Should().Be("redis-password");
    }

    [Theory]
    [InlineData("localhost", 6379)]
    [InlineData("redis.example.com", 6379)]
    [InlineData("192.168.1.50", 6380)]
    [InlineData("cache-server", 7000)]
    public void RedisOptions_ShouldHandleVariousHostsAndPorts(string host, int port)
    {
        // Arrange & Act
        var options = new RedisOptions.RedisOptions
        {
            Host = host,
            Port = port
        };

        // Assert
        options.Host.Should().Be(host);
        options.Port.Should().Be(port);
    }

    [Fact]
    public void RedisOptions_ShouldAllowNullPassword()
    {
        // Arrange & Act
        var options = new RedisOptions.RedisOptions
        {
            Host = "localhost",
            Port = 6379,
            Password = null
        };

        // Assert
        options.Password.Should().BeNull();
    }

    [Fact]
    public void RedisOptions_ShouldAllowEmptyPassword()
    {
        // Arrange & Act
        var options = new RedisOptions.RedisOptions
        {
            Host = "localhost",
            Port = 6379,
            Password = ""
        };

        // Assert
        options.Password.Should().BeEmpty();
    }
}

public class PostgresCpOptionsTests
{
    [Fact]
    public void PostgresCpOptions_DefaultConstructor_ShouldInitializeProperties()
    {
        // Act
        var options = new PostgresCpOptions();

        // Assert
        options.Should().NotBeNull();
        options.ConnectionString.Should().BeNull();
    }

    [Fact]
    public void PostgresCpOptions_ConnectionString_ShouldBeSettable()
    {
        // Arrange
        var options = new PostgresCpOptions();
        var connectionString = "Host=localhost;Database=testdb;Username=user;Password=pass";

        // Act
        options.ConnectionString = connectionString;

        // Assert
        options.ConnectionString.Should().Be(connectionString);
    }

    [Theory]
    [InlineData("Host=localhost;Database=db1;Username=user1;Password=pass1")]
    [InlineData("Server=127.0.0.1;Port=5432;Database=mydb;User Id=admin;Password=secret")]
    [InlineData("Host=postgres.example.com;Database=production;Username=app_user;Password=strong_password")]
    public void PostgresCpOptions_ShouldHandleVariousConnectionStrings(string connectionString)
    {
        // Arrange & Act
        var options = new PostgresCpOptions
        {
            ConnectionString = connectionString
        };

        // Assert
        options.ConnectionString.Should().Be(connectionString);
    }

    [Fact]
    public void PostgresCpOptions_ShouldAllowEmptyConnectionString()
    {
        // Arrange & Act
        var options = new PostgresCpOptions
        {
            ConnectionString = ""
        };

        // Assert
        options.ConnectionString.Should().BeEmpty();
    }

    [Fact]
    public void PostgresCpOptions_ShouldAllowLongConnectionStrings()
    {
        // Arrange
        var longConnectionString = new string('a', 500);

        // Act
        var options = new PostgresCpOptions
        {
            ConnectionString = longConnectionString
        };

        // Assert
        options.ConnectionString.Should().HaveLength(500);
        options.ConnectionString.Should().Be(longConnectionString);
    }
}
