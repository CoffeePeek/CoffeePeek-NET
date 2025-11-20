using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Domain.Enums.Shop;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoffeePeek.Web.Pages;

public class ModerationModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ModerationModel> _logger;

    public ModerationModel(
        IHttpClientFactory httpClientFactory, 
        IConfiguration configuration,
        ILogger<ModerationModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public List<ModerationShopDto> ReviewShops { get; set; } = new();
    public bool IsAuthenticated { get; set; }
    public string? AccessToken { get; set; }

    [TempData]
    public string? SuccessMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync(string? filter = null)
    {
        LoadUserData();
        
        ViewData["IsAuthenticated"] = IsAuthenticated;
        ViewData["UserEmail"] = HttpContext.Session.GetString("UserEmail");

        if (!IsAuthenticated)
        {
            Response.Redirect("/Login");
            return;
        }

        await LoadReviewShopsAsync(filter);
    }

    private void LoadUserData()
    {
        AccessToken = HttpContext.Session.GetString("AccessToken") ?? 
                     HttpContext.Request.Cookies["AccessToken"];
        IsAuthenticated = !string.IsNullOrEmpty(AccessToken);
    }

    private async Task LoadReviewShopsAsync(string? filter = null)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7001";

            if (!string.IsNullOrEmpty(AccessToken))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            }

            var response = await client.GetAsync($"{apiBaseUrl}/api/CoffeeShopReview/all");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<GetReviewShopsResponse>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (apiResponse?.IsSuccess == true && apiResponse.Data != null)
                {
                    var allShops = apiResponse.Data.ReviewShops?.ToList() ?? new List<ModerationShopDto>();
                    
                    // Фильтрация по статусу
                    if (!string.IsNullOrEmpty(filter))
                    {
                        if (Enum.TryParse<ModerationStatus>(filter, out var statusFilter))
                        {
                            ReviewShops = allShops.Where(s => s.ModerationStatus == statusFilter).ToList();
                        }
                        else
                        {
                            ReviewShops = allShops;
                        }
                    }
                    else
                    {
                        // По умолчанию показываем только Pending
                        ReviewShops = allShops.Where(s => s.ModerationStatus == ModerationStatus.Pending).ToList();
                    }
                }
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                ErrorMessage = "Вы не авторизованы. Пожалуйста, войдите в систему.";
                Response.Redirect("/Login");
                return;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error loading review shops: {StatusCode} - {Content}", response.StatusCode, errorContent);
                ErrorMessage = "Ошибка при загрузке данных для модерации.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception loading review shops");
            ErrorMessage = $"Ошибка: {ex.Message}";
        }
    }

    public async Task<IActionResult> OnPostUpdateStatusAsync(int id, string status)
    {
        LoadUserData();

        if (!IsAuthenticated)
        {
            ErrorMessage = "Вы не авторизованы.";
            return RedirectToPage();
        }

        // Парсим строку в enum
        if (!Enum.TryParse<ModerationStatus>(status, out var reviewStatus))
        {
            ErrorMessage = "Неверный статус.";
            return RedirectToPage();
        }

        try
        {
            var client = _httpClientFactory.CreateClient();
            var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7001";

            if (!string.IsNullOrEmpty(AccessToken))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            }

            var statusString = reviewStatus.ToString();
            var response = await client.PutAsync(
                $"{apiBaseUrl}/api/CoffeeShopReview/status?id={id}&status={statusString}", 
                null);

            if (response.IsSuccessStatusCode)
            {
                SuccessMessage = $"Статус успешно обновлен на {reviewStatus}.";
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error updating status: {StatusCode} - {Content}", response.StatusCode, errorContent);
                ErrorMessage = "Ошибка при обновлении статуса.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception updating status");
            ErrorMessage = $"Ошибка: {ex.Message}";
        }

        return RedirectToPage();
    }
}

// Response models
public class ApiResponse<T>
{
    [JsonPropertyName(("Success"))]
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
}

public class GetReviewShopsResponse
{
    public ModerationShopDto[]? ReviewShops { get; set; }
}

