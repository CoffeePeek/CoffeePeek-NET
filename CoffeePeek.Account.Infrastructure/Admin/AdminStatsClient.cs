using System.Net.Http.Json;
using CoffeePeek.Account.Application.Features.Admin.Stats;
using CoffeePeek.Contract.Dtos.Admin;
using CoffeePeek.Shared.Auth.Constants;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CoffeePeek.Account.Infrastructure.Admin;

public class AdminStatsClient(
    IHttpClientFactory httpClientFactory,
    IHttpContextAccessor httpContextAccessor,
    ILogger<AdminStatsClient> logger) : IAdminStatsClient
{
    public async Task<AdminServiceStatsDto> GetPlatformStatsAsync(CancellationToken ct = default)
    {
        var shopsStats = await FetchStatsAsync(AppResources.ShopsService, ct);
        var moderationStats = await FetchStatsAsync(AppResources.ModerationService, ct);

        return new AdminServiceStatsDto(
            TotalCoffeeShops: shopsStats.TotalCoffeeShops,
            TotalReviews: shopsStats.TotalReviews,
            NewCoffeeShopsToday: shopsStats.NewCoffeeShopsToday,
            NewReviewsToday: shopsStats.NewReviewsToday,
            PendingModerationShops: moderationStats.PendingModerationShops,
            PendingModerationReviews: moderationStats.PendingModerationReviews);
    }

    private async Task<AdminServiceStatsDto> FetchStatsAsync(string serviceName, CancellationToken ct)
    {
        try
        {
            var client = httpClientFactory.CreateClient("admin-stats");
            using var request = new HttpRequestMessage(HttpMethod.Get, $"http://{serviceName}/api/admin/stats/summary");
            ForwardAuthHeaders(request);

            using var response = await client.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Admin stats request to {Service} failed with {StatusCode}", serviceName, response.StatusCode);
                return new AdminServiceStatsDto();
            }

            var payload = await response.Content.ReadFromJsonAsync<Response<AdminServiceStatsDto>>(cancellationToken: ct);
            return payload?.Data ?? new AdminServiceStatsDto();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Admin stats request to {Service} failed", serviceName);
            return new AdminServiceStatsDto();
        }
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
