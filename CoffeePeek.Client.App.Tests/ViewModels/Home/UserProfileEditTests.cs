using Avalonia.Threading;
using CoffeePeek.Client.App.Core.Cache;
using CoffeePeek.Client.App.Core.Identity;
using CoffeePeek.Client.App.Core.Settings;
using CoffeePeek.Client.App.Infrastructure.HTTP.Configuration;
using CoffeePeek.Client.App.Infrastructure.HTTP.Responses;
using CoffeePeek.Client.App.Infrastructure.HTTP.WebClients;
using CoffeePeek.Client.App.Services;
using CoffeePeek.Client.App.ViewModels.Home;
using FluentAssertions;
using FluentResults;
using Moq;
using Xunit;

namespace CoffeePeek.Client.App.Tests.ViewModels.Home;

public class UserProfileEditTests
{
    private readonly Mock<IWebUserProfileClient> _profileClientMock = new();
    private readonly Mock<IWebUserReviewsClient> _reviewsClientMock = new();
    private readonly Mock<IUserIdentityAccessor> _identityMock = new();
    private readonly Mock<IWorkspaceShellNavigator> _navigatorMock = new();
    private readonly Mock<IThemeController> _themeMock = new();
    private readonly Mock<IClientSession> _sessionMock = new();
    private readonly Mock<INavigationService> _navMock = new();
    private readonly Mock<IWebAuthenticationClient> _authClientMock = new();
    private readonly Mock<ILocalUserSettings> _settingsMock = new();

    private static readonly Guid OwnUserId = Guid.NewGuid();

    private UserProfileViewModel CreateSut(Guid? currentUserId = null)
    {
        _identityMock
            .Setup(i => i.GetCurrentUserIdOrNull())
            .Returns(currentUserId);

        return new UserProfileViewModel(
            new HttpClient(),
            new ApiOptions { BaseAddress = "https://test.local" },
            _profileClientMock.Object,
            _reviewsClientMock.Object,
            _themeMock.Object,
            _navigatorMock.Object,
            _sessionMock.Object,
            _navMock.Object,
            _authClientMock.Object,
            _settingsMock.Object,
            _identityMock.Object);
    }

    private void SetupProfileSuccess(string userName = "testuser", string? about = "some bio")
    {
        _profileClientMock
            .Setup(c => c.GetPublicProfileAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(new UserProfileDto
            {
                UserName = userName,
                Email = "test@test.com",
                About = about,
                CreatedAtUtc = new DateTime(2024, 1, 1),
                ReviewCount = 5,
                CheckInCount = 3
            }));

        _reviewsClientMock
            .Setup(c => c.GetReviewsAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(new GetReviewsByUserIdResultDto
            {
                ReviewDtos = [],
                TotalPages = 1,
                CurrentPage = 1
            }));
    }

    [Fact]
    public void StartEditing_CopiesCurrentValuesToEditFields()
    {
        var sut = CreateSut(OwnUserId);
        sut.UserName = "alice";
        sut.About = "hello world";

        sut.StartEditingCommand.Execute(null);

        sut.IsEditing.Should().BeTrue();
        sut.EditUserName.Should().Be("alice");
        sut.EditAbout.Should().Be("hello world");
    }

    [Fact]
    public void CancelEditing_SetsIsEditingFalse()
    {
        var sut = CreateSut(OwnUserId);
        sut.StartEditingCommand.Execute(null);

        sut.CancelEditingCommand.Execute(null);

        sut.IsEditing.Should().BeFalse();
    }

    [Fact]
    public async Task SaveProfile_UpdatesUsername_WhenChanged()
    {
        SetupProfileSuccess("oldname", "bio");
        _profileClientMock
            .Setup(c => c.UpdateUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        var sut = CreateSut(OwnUserId);
        sut.UserName = "oldname";
        sut.About = "bio";
        sut.StartEditingCommand.Execute(null);
        sut.EditUserName = "newname";

        await sut.SaveProfileCommand.ExecuteAsync(null);

        sut.UserName.Should().Be("newname");
        sut.IsEditing.Should().BeFalse();
        _profileClientMock.Verify(c => c.UpdateUsernameAsync("newname", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SaveProfile_UpdatesAbout_WhenChanged()
    {
        SetupProfileSuccess("user", "old bio");
        _profileClientMock
            .Setup(c => c.UpdateAboutAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        var sut = CreateSut(OwnUserId);
        sut.UserName = "user";
        sut.About = "old bio";
        sut.StartEditingCommand.Execute(null);
        sut.EditAbout = "new bio";

        await sut.SaveProfileCommand.ExecuteAsync(null);

        sut.About.Should().Be("new bio");
        sut.IsEditing.Should().BeFalse();
        _profileClientMock.Verify(c => c.UpdateAboutAsync("new bio", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SaveProfile_DoesNotCallApi_WhenNothingChanged()
    {
        var sut = CreateSut(OwnUserId);
        sut.UserName = "same";
        sut.About = "same bio";
        sut.StartEditingCommand.Execute(null);

        await sut.SaveProfileCommand.ExecuteAsync(null);

        _profileClientMock.Verify(c => c.UpdateUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _profileClientMock.Verify(c => c.UpdateAboutAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        sut.IsEditing.Should().BeFalse();
    }

    [Fact]
    public async Task SaveProfile_ShowsError_WhenUsernameFails()
    {
        _profileClientMock
            .Setup(c => c.UpdateUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail("Username taken"));

        var sut = CreateSut(OwnUserId);
        sut.UserName = "old";
        sut.About = "";
        sut.StartEditingCommand.Execute(null);
        sut.EditUserName = "new";

        await sut.SaveProfileCommand.ExecuteAsync(null);

        sut.HasEditError.Should().BeTrue();
        sut.EditErrorMessage.Should().Be("Username taken");
        sut.IsEditing.Should().BeTrue();
    }

    [Fact]
    public async Task SaveProfile_ShowsError_WhenUsernameEmpty()
    {
        var sut = CreateSut(OwnUserId);
        sut.UserName = "old";
        sut.About = "";
        sut.StartEditingCommand.Execute(null);
        sut.EditUserName = "   ";

        await sut.SaveProfileCommand.ExecuteAsync(null);

        sut.HasEditError.Should().BeTrue();
        sut.IsEditing.Should().BeTrue();
    }

    [Fact]
    public void IsOwnProfile_False_WhenViewingOtherUser()
    {
        var sut = CreateSut(OwnUserId);
        sut.IsOwnProfile.Should().BeFalse();
    }
}
