using CoffeePeek.AuthService.Commands;
using CoffeePeek.AuthService.Entities;
using CoffeePeek.AuthService.Repositories;
using CoffeePeek.Contract.Response.Auth;
using CoffeePeek.Contract.Response.Login;
using CoffeePeek.Contract.Responses;
using FluentAssertions;
using CoffeePeek.AuthService.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace CoffeePeek.AuthService.Tests.Integration;

public class AuthFlowIntegrationTests(
    AuthServiceWebApplicationFactory factory)
    : IClassFixture<AuthServiceWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Register_Login_Refresh_Flow_Works()
    {
        await ClearDatabaseAsync();

        // Arrange
        var email = $"user{Guid.NewGuid():N}@gmail.com";
        var password = "StrongP@ssw0rd!";
        var userName = "Test User";

        // Register
        var registerCommand = new RegisterUserCommand(userName, email, password);
        var registerResponse = await _client.PostAsJsonAsync("/api/Auth/register", registerCommand);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var registerResult = await registerResponse.Content.ReadFromJsonAsync<CreateEntityResponse<Guid>>();
        registerResult.Should().NotBeNull();
        registerResult!.IsSuccess.Should().BeTrue();
        registerResult.Data.Should().NotBe(Guid.Empty);
        var userId = registerResult.Data;

        // Login
        var loginResponse = await _client.PostAsJsonAsync("/api/Auth/login", new LoginUserCommand(email, password));
        if (loginResponse.StatusCode != HttpStatusCode.OK)
        {
            var errorBody = await loginResponse.Content.ReadAsStringAsync();
            Assert.Fail($"Unexpected status {loginResponse.StatusCode}: {errorBody}");
        }
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<Response<LoginResponse>>();
        loginResult.Should().NotBeNull();
        loginResult!.IsSuccess.Should().BeTrue();
        loginResult.Data.Should().NotBeNull();

        var refreshToken = loginResult.Data!.RefreshToken;

        // Refresh (with test auth header - use the registered user's ID)
        var refreshRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/Auth/refresh?refreshToken={refreshToken}");
        refreshRequest.Headers.Add("X-Test-UserId", userId.ToString());
        refreshRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Test");

        var refreshResponse = await _client.SendAsync(refreshRequest);
        if (refreshResponse.StatusCode != HttpStatusCode.OK)
        {
            var errorBody = await refreshResponse.Content.ReadAsStringAsync();
            Assert.Fail($"Unexpected status {refreshResponse.StatusCode}: {errorBody}");
        }

        var refreshResult = await refreshResponse.Content.ReadFromJsonAsync<Response<GetRefreshTokenResponse>>();
        refreshResult.Should().NotBeNull();
        if (!refreshResult!.IsSuccess)
        {
            Assert.Fail($"Refresh token failed: {refreshResult.Message}");
        }
        refreshResult.IsSuccess.Should().BeTrue();
        refreshResult.Data.Should().NotBeNull();
        refreshResult.Data!.AccessToken.Should().NotBeNullOrWhiteSpace();
        refreshResult.Data.RefreshToken.Should().NotBeNullOrWhiteSpace();
    }

    private async Task ClearDatabaseAsync()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        var roleRepository = scope.ServiceProvider.GetRequiredService<IRoleRepository>();

        db.RefreshTokens.RemoveRange(db.RefreshTokens);
        db.UserRoles.RemoveRange(db.UserRoles);
        db.Users.RemoveRange(db.Users);
        db.Roles.RemoveRange(db.Roles);
        await db.SaveChangesAsync();

        // Ensure required roles exist (create directly to avoid RoleExistsAsync issue with StringComparison)
        var userRole = await roleRepository.GetRoleAsync(CoffeePeek.Shared.Infrastructure.Constants.RoleConsts.User);
        if (userRole == null)
        {
            var role = new Role
            {
                Name = CoffeePeek.Shared.Infrastructure.Constants.RoleConsts.User
            };
            await roleRepository.CreateRoleAsync(role);
            await db.SaveChangesAsync();
        }
    }
}

