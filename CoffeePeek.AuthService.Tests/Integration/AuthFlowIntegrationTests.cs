using CoffeePeek.AuthService.Commands;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.Login;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using CoffeePeek.Auth.Application.Commands;
using CoffeePeek.Auth.Domain.Entities;
using CoffeePeek.Auth.Domain.Repositories;
using CoffeePeek.Auth.Infrastructure.Configuration;
using CoffeePeek.Contract.Responses.Auth;
using CoffeePeek.Shared.Infrastructure.Constants;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CoffeePeek.AuthService.Tests.Integration;

public class AuthFlowIntegrationTests(AuthServiceWebApplicationFactory factory) : IClassFixture<AuthServiceWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Register_Login_Refresh_Flow_Works()
    {
        await ClearDatabaseAsync();

        // Arrange
        var email = $"user{Guid.NewGuid():N}@gmail.com";
        const string password = "StrongP@ssw0rd!";
        const string userName = "Test User";

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
        refreshToken.Should().NotBeNullOrWhiteSpace("Refresh token should be returned from login");

        // Verify the refresh token was saved to the database
        using (var verifyScope = factory.Services.CreateScope())
        {
            var verifyDb = verifyScope.ServiceProvider.GetRequiredService<AuthDbContext>();
            var savedToken = await verifyDb.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken && rt.UserId == userId);
            savedToken.Should().NotBeNull("Refresh token should be saved to database after login");
        }

        // Refresh (with test auth header - use the registered user's ID)
        var refreshRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/Auth/refresh?refreshToken={Uri.EscapeDataString(refreshToken)}");
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
        var userRole = await roleRepository.GetRoleAsync(RoleConsts.User);
        if (userRole == null)
        {
            var role = new Role
            {
                Name = RoleConsts.User
            };
            await roleRepository.CreateRoleAsync(role);
            await db.SaveChangesAsync();
        }
    }
}

