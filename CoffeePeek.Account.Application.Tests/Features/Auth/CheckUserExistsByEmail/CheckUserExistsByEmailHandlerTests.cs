using System.Threading;
using System.Threading.Tasks;
using CoffeePeek.Account.Application.Common;
using CoffeePeek.Account.Application.Features.Auth.CheckUserExistsByEmail;
using CoffeePeek.Account.Domain.Entities.UserAggregate;
using FluentAssertions;
using Moq;
using Xunit;

namespace CoffeePeek.Account.Application.Tests.Features.Auth.CheckUserExistsByEmail;

public class CheckUserExistsByEmailHandlerTests
{
    private readonly Mock<IQueryUserRepository> _userRepoMock = new();
    private readonly EmailExistenceFilter _filter = new(1000, 0.01);
    private readonly CancellationToken _ct = CancellationToken.None;

    [Fact]
    public async Task Handle_WhenUserExists_ReturnsSuccessTrue()
    {
        const string email = "user@example.com";
        _userRepoMock.Setup(r => r.UserExistsByEmail(email, _ct)).ReturnsAsync(true);

        var response = await CheckUserExistsByEmailRequestHandler.Handle(
            new CheckUserExistsByEmailCommand(email), _userRepoMock.Object, _filter, _ct);

        response.IsSuccess.Should().BeTrue();
        response.Data.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ReturnsSuccessFalse()
    {
        const string email = "nobody@example.com";
        _userRepoMock.Setup(r => r.UserExistsByEmail(email, _ct)).ReturnsAsync(false);

        var response = await CheckUserExistsByEmailRequestHandler.Handle(
            new CheckUserExistsByEmailCommand(email), _userRepoMock.Object, _filter, _ct);

        response.IsSuccess.Should().BeTrue();
        response.Data.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenBloomFilterHitsButDbSaysMissing_ReturnsSuccessFalse()
    {
        const string email = "ghost@example.com";
        _filter.Add(email);
        _userRepoMock.Setup(r => r.UserExistsByEmail(email, _ct)).ReturnsAsync(false);

        var response = await CheckUserExistsByEmailRequestHandler.Handle(
            new CheckUserExistsByEmailCommand(email), _userRepoMock.Object, _filter, _ct);

        response.IsSuccess.Should().BeTrue();
        response.Data.Should().BeFalse();
        _userRepoMock.Verify(r => r.UserExistsByEmail(email, _ct), Times.Once);
    }
}
