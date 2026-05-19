using CoffeePeek.Account.Application.Common;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace CoffeePeek.Account.Application.Tests.Common;

public class EmailExistenceFilterTests
{
    private static EmailExistenceFilter CreateFilter(int capacity = 1000) => new(capacity, 0.01);

    [Fact]
    public void MightExist_ForUnseenEmail_ReturnsFalse()
    {
        // Arrange
        var filter = CreateFilter();

        // Act & Assert
        filter.MightExist("nobody@example.com").Should().BeFalse();
    }

    [Fact]
    public void MightExist_AfterAdd_ReturnsTrue()
    {
        // Arrange
        var filter = CreateFilter();
        const string email = "user@example.com";

        // Act
        filter.Add(email);

        // Assert
        filter.MightExist(email).Should().BeTrue();
    }

    [Fact]
    public void MightExist_IsCaseInsensitive()
    {
        // Arrange
        var filter = CreateFilter();
        filter.Add("User@Example.COM");

        // Act & Assert
        filter.MightExist("user@example.com").Should().BeTrue();
        filter.MightExist("USER@EXAMPLE.COM").Should().BeTrue();
    }

    [Fact]
    public void MightExist_TrimsWhitespace()
    {
        // Arrange
        var filter = CreateFilter();
        filter.Add("  user@example.com  ");

        // Act & Assert
        filter.MightExist("user@example.com").Should().BeTrue();
    }

    [Fact]
    public void Add_CalledMultipleTimes_DoesNotCrash()
    {
        // Arrange
        var filter = CreateFilter();
        const string email = "repeat@example.com";

        // Act
        for (var i = 0; i < 10; i++) filter.Add(email);

        // Assert
        filter.MightExist(email).Should().BeTrue();
    }

    [Fact]
    public void MightExist_ForDifferentEmail_ReturnsFalse()
    {
        // Arrange
        var filter = CreateFilter();
        filter.Add("alice@example.com");

        // Act & Assert
        filter.MightExist("bob@example.com").Should().BeFalse();
    }

    [Fact]
    public void Filter_HandlesLargeNumberOfEmails()
    {
        // Arrange
        var filter = new EmailExistenceFilter(10_000, 0.01);

        // Act — add 1000 emails
        var emails = Enumerable.Range(1, 1000).Select(i => $"user{i}@example.com").ToList();
        foreach (var e in emails) filter.Add(e);

        // Assert — all added emails are found (no false negatives)
        emails.Should().AllSatisfy(e => filter.MightExist(e).Should().BeTrue());
    }

    [Theory]
    [InlineData("a@b.com")]
    [InlineData("test+tag@domain.org")]
    [InlineData("very.long.email.address@subdomain.example.co.uk")]
    public void MightExist_AfterAdd_ReturnsTrueForVariousFormats(string email)
    {
        // Arrange
        var filter = CreateFilter();
        filter.Add(email);

        // Act & Assert
        filter.MightExist(email).Should().BeTrue();
    }
}
