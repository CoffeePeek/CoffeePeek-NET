using System.Net.Http.Json;
using CoffeePeek.Account.Application.Features.Admin.Stats;
using CoffeePeek.Account.Infrastructure.Options;
using CoffeePeek.Contract.Dtos.Admin;
using CoffeePeek.Shared.Auth.Constants;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CoffeePeek.Account.Infrastructure.Admin;

public class AdminStatsClient(
    IHttpClientFactory httpClientFactory,
    IHttpContextAccessor httpContextAccessor,
    IOptions<AdminStatsOptions> options,
    ILogger<AdminStatsClient> logger) : IAdminStatsClient
{
    public async Task<AdminServiceStatsDto> GetPlatformStatsAsync(CancellationToken cancellationToken = default)
    {
        var shopsStats = await FetchStatsAsync(
            options.Value.ShopsServiceUrl,
            AppResources.ShopsService,
            cancellationToken);

        var moderationStats = await FetchStatsAsync(
            options.Value.ModerationServiceUrl,
            AppResources.ModerationService,
            cancellationToken);

        return new AdminServiceStatsDto(
            TotalCoffeeShops: shopsStats.TotalCoffeeShops,
            TotalReviews: shopsStats.TotalReviews,
            NewCoffeeShopsToday: shopsStats.NewCoffeeShopsToday,
            NewReviewsToday: shopsStats.NewReviewsToday,
            PendingModerationShops: moderationStats.PendingModerationShops,
            PendingModerationReviews: moderationStats.PendingModerationReviews);
    }

    private async Task<AdminServiceStatsDto> FetchStatsAsync(
        string? configuredBaseUrl,
        string aspireServiceName,
        CancellationToken cancellationToken)
    {
        var baseUrl = ResolveBaseUrl(configuredBaseUrl, aspireServiceName);
        var client = httpClientFactory.CreateClient("admin-stats");
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{baseUrl}/api/admin/stats/summary");
        ForwardAuthHeaders(request);

        using var response = await client.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning(
                "Admin stats request to {BaseUrl} failed with {StatusCode}",
                baseUrl,
                response.StatusCode);
            throw new HttpRequestException($"Admin stats request to {baseUrl} failed with {response.StatusCode}");
        }

        var payload = await response.Content.ReadFromJsonAsync<Response<AdminServiceStatsDto>>(
            cancellationToken: cancellationToken);

        if (payload is not { IsSuccess: true, Data: not null })
            throw new InvalidOperationException($"Admin stats request to {baseUrl} returned an invalid payload.");

        return payload.Data;
    }

    private static string ResolveBaseUrl(string? configuredBaseUrl, string aspireServiceName)
    {
        if (!string.IsNullOrWhiteSpace(configuredBaseUrl))
            return configuredBaseUrl.TrimEnd('/');

        // Local Aspire: service discovery resolves this logical name inside the dev network.
        return $"http://{aspireServiceName}";
    }

    private void ForwardAuthHeaders(HttpRequestMessage request)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext is null)
            return;

        foreach (var headerName in new[]
                 {
                     GatewayHeaderConsts.XUserId,
                     GatewayHeaderConsts.XUserName,
                     GatewayHeaderConsts.XUserRole
                 })
        {
            if (httpContext.Request.Headers.TryGetValue(headerName, out var value))
                request.Headers.TryAddWithoutValidation(headerName, value.ToArray());
        }
    }
}
