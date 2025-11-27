using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Domain.Enums.Shop;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoffeePeek.Web.Pages;

public class EditReviewModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EditReviewModel> _logger;

    public EditReviewModel(
        IHttpClientFactory httpClientFactory, 
        IConfiguration configuration,
        ILogger<EditReviewModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public ModerationShopDto? ModerationShop { get; set; }
    public bool IsAuthenticated { get; set; }
    public string? AccessToken { get; set; }

    [TempData]
    public string? SuccessMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync(int id)
    {
        LoadUserData();
        
        ViewData["IsAuthenticated"] = IsAuthenticated;
        ViewData["UserEmail"] = HttpContext.Session.GetString("UserEmail");

        if (!IsAuthenticated)
        {
            Response.Redirect("/Login");
            return;
        }

        await LoadReviewShopAsync(id);
    }

    private void LoadUserData()
    {
        AccessToken = HttpContext.Session.GetString("AccessToken") ?? 
                     HttpContext.Request.Cookies["AccessToken"];
        IsAuthenticated = !string.IsNullOrEmpty(AccessToken);
    }

    private async Task LoadReviewShopAsync(int id)
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
                    ModerationShop = allShops.FirstOrDefault(s => s.Id == id);
                    
                    if (ModerationShop == null)
                    {
                        ErrorMessage = "Заявка не найдена.";
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
                _logger.LogError("Error loading review shop: {StatusCode} - {Content}", response.StatusCode, errorContent);
                ErrorMessage = "Ошибка при загрузке данных заявки.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception loading review shop");
            ErrorMessage = $"Ошибка: {ex.Message}";
        }
    }

    public async Task<IActionResult> OnPostUpdateStatusAsync(int id, string status)
    {
        LoadUserData();

        if (!IsAuthenticated)
        {
            ErrorMessage = "Вы не авторизованы.";
            return RedirectToPage(new { id });
        }

        // Парсим строку в enum
        if (!Enum.TryParse<ModerationStatus>(status, out var reviewStatus))
        {
            ErrorMessage = "Неверный статус.";
            return RedirectToPage(new { id });
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

        return RedirectToPage(new { id });
    }
}

