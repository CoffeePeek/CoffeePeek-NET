using System.Text.Json;
using CoffeePeek.JobVacancies.Configuration;
using CoffeePeek.JobVacancies.Models;
using CoffeePeek.JobVacancies.Models.Responses;
using Microsoft.Extensions.Options;

namespace CoffeePeek.JobVacancies.Services;

public class HhApiService : IHhApiService
{
    private readonly HhApiOptions _apiOptions;
    private readonly IHhAuthService _authService;
    private readonly HttpClient _httpClient;
    
    public HhApiService(IOptions<HhApiOptions> options, IHhAuthService authService, HttpClient httpClient)
    {
        _apiOptions = options.Value;
        _authService = authService;
        _httpClient = httpClient;
        
        if (httpClient.BaseAddress == null)
            httpClient.BaseAddress = new Uri(_apiOptions.BaseUrl);

        if (httpClient.DefaultRequestHeaders.UserAgent.Count == 0)
            httpClient.DefaultRequestHeaders.Add("User-Agent", _apiOptions.UserAgent);
    }

    private async Task EnsureAccessTokenAsync(CancellationToken cancellationToken)
    {
        var token = await _authService.GetAccessTokenAsync(cancellationToken);
        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }
    
    public async Task<List<HhAreaNode>> GetAreas(CancellationToken cancellationToken = default)
    {
        //await EnsureAccessTokenAsync(cancellationToken);
        
        var url = $"{_apiOptions.BaseUrl.TrimEnd('/')}/areas";

        var response = await _httpClient.GetAsync(url,cancellationToken);
        response.EnsureSuccessStatusCode();

        var areas = await response.Content.ReadFromJsonAsync<List<HhAreaNode>>(
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
            cancellationToken
        );

        return areas ?? [];
    }
    
    public async Task<List<HhVacancyItem>> GetVacancies(IEnumerable<string> texts, int[] areaIds, CancellationToken cancellationToken = default)
    {
        //await EnsureAccessTokenAsync(cancellationToken);
        
        var allItems = new List<HhVacancyItem>();

        foreach (var text in texts)
        {
            foreach (var area in areaIds)
            {
                var url = $"vacancies?text={Uri.EscapeDataString(text)}&host={Uri.EscapeDataString("rabota.by")}&area={area}&per_page=100";
                using var response = await _httpClient.GetAsync(url, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync(cancellationToken);
                    throw new HttpRequestException($"hh.ru returned {(int)response.StatusCode}: {body}");
                }

                var hhResponse = await response.Content.ReadFromJsonAsync<HhVacancyResponse>(
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, 
                    cancellationToken
                ) ?? new HhVacancyResponse();

                allItems.AddRange(hhResponse.Items.Where(x => areaIds.Contains(int.Parse(x.Area.Id))));
            }
        }

        return allItems;
    }
}