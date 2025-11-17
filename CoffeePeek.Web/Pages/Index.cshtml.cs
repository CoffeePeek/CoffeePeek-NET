using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using CoffeePeek.Web.Contract.Http;
using CoffeePeek.Web.Contract.Http.Shops;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoffeePeek.Web.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public IndexModel(ILogger<IndexModel> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public GetCoffeeShopsResponse? ResponseData { get; private set; }
    public bool IsAuthenticated { get; set; }
    public string? UserEmail { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }

    [TempData]
    public string? RedisResult { get; set; }

    [TempData]
    public string? EmailCheckResult { get; set; }

    [TempData]
    public string? RefreshTokenResult { get; set; }

    [TempData]
    public string? CustomApiResult { get; set; }

    public async Task OnGetAsync()
    {
        LoadUserData();
        ViewData["IsAuthenticated"] = IsAuthenticated;
        ViewData["UserEmail"] = UserEmail;
        await OnGetCoffeeShopsAsync();
    }

    private void LoadUserData()
    {
        AccessToken = HttpContext.Session.GetString("AccessToken");
        RefreshToken = HttpContext.Session.GetString("RefreshToken");
        UserEmail = HttpContext.Session.GetString("UserEmail");
        IsAuthenticated = !string.IsNullOrEmpty(AccessToken);
    }

    public async Task OnGetCoffeeShopsAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7001";
            
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
            client.DefaultRequestHeaders.Add("X-Page-Number", "1");
            client.DefaultRequestHeaders.Add("X-Page-Size", "10");

            var response = await client.GetAsync($"{apiBaseUrl}/api/CoffeeShop?cityId=1");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            
            ResponseData = JsonSerializer.Deserialize<ApiResponse<GetCoffeeShopsResponse>>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                })?.Data;
            
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching coffee shops");
        }
    }
        
    public async Task<IActionResult> OnPostRedisTestAsync(string redisAction, string redisKey, string? redisValue)
    {
        LoadUserData();

        try
        {
            var client = _httpClientFactory.CreateClient();
            var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7001";

            // Добавляем токен авторизации если есть
            if (!string.IsNullOrEmpty(AccessToken))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            }

            switch (redisAction.ToLower())
            {
                case "set":
                    var setData = new { Key = redisKey, Value = redisValue };
                    var setContent = new StringContent(JsonSerializer.Serialize(setData), Encoding.UTF8, "application/json");
                    var setResponse = await client.PostAsync($"{apiBaseUrl}/api/Test/redis", setContent);
                    RedisResult = await FormatResponse(setResponse);
                    break;

                case "get":
                    var getResponse = await client.GetAsync($"{apiBaseUrl}/api/Test/redis/{redisKey}");
                    RedisResult = await FormatResponse(getResponse);
                    break;

                case "delete":
                    var deleteResponse = await client.DeleteAsync($"{apiBaseUrl}/api/Test/redis/{redisKey}");
                    RedisResult = await FormatResponse(deleteResponse);
                    break;

                default:
                    RedisResult = "Invalid action selected";
                    break;
            }
        }
        catch (Exception ex)
        {
            RedisResult = $"Error: {ex.Message}";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCheckEmailAsync(string emailToCheck)
    {
        LoadUserData();

        try
        {
            var client = _httpClientFactory.CreateClient();
            var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7001";

            var response = await client.PostAsync($"{apiBaseUrl}/api/Auth/check-exists?email={Uri.EscapeDataString(emailToCheck)}", null);
            EmailCheckResult = await FormatResponse(response);
        }
        catch (Exception ex)
        {
            EmailCheckResult = $"Error: {ex.Message}";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRefreshTokenAsync(string refreshToken)
    {
        LoadUserData();

        try
        {
            var client = _httpClientFactory.CreateClient();
            var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7001";

            var response = await client.GetAsync($"{apiBaseUrl}/api/Auth/refresh?refreshToken={Uri.EscapeDataString(refreshToken)}");
            var result = await FormatResponse(response);

            if (response.IsSuccessStatusCode)
            {
                // Пытаемся обновить токен в сессии
                try
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var refreshResponse = JsonSerializer.Deserialize<RefreshTokenApiResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (refreshResponse?.IsSuccess == true && refreshResponse.Data != null)
                    {
                        HttpContext.Session.SetString("AccessToken", refreshResponse.Data.AccessToken);
                        HttpContext.Session.SetString("RefreshToken", refreshResponse.Data.RefreshToken);
                        result += "\n\n✅ Tokens updated in session!";
                    }
                }
                catch { }
            }

            RefreshTokenResult = result;
        }
        catch (Exception ex)
        {
            RefreshTokenResult = $"Error: {ex.Message}";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCustomApiAsync(string httpMethod, string apiEndpoint, string? apiBody, bool includeAuth)
    {
        LoadUserData();

        try
        {
            var client = _httpClientFactory.CreateClient();
            var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7001";

            // Добавляем токен авторизации если требуется
            if (includeAuth && !string.IsNullOrEmpty(AccessToken))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            }

            var fullUrl = apiBaseUrl + apiEndpoint;
            HttpResponseMessage response;

            switch (httpMethod.ToUpper())
            {
                case "GET":
                    response = await client.GetAsync(fullUrl);
                    break;

                case "POST":
                    var postContent = string.IsNullOrWhiteSpace(apiBody) 
                        ? null 
                        : new StringContent(apiBody, Encoding.UTF8, "application/json");
                    response = await client.PostAsync(fullUrl, postContent);
                    break;

                case "PUT":
                    var putContent = string.IsNullOrWhiteSpace(apiBody) 
                        ? null 
                        : new StringContent(apiBody, Encoding.UTF8, "application/json");
                    response = await client.PutAsync(fullUrl, putContent);
                    break;

                case "DELETE":
                    response = await client.DeleteAsync(fullUrl);
                    break;

                default:
                    CustomApiResult = "Invalid HTTP method";
                    return RedirectToPage();
            }

            CustomApiResult = $"Status: {(int)response.StatusCode} {response.StatusCode}\n\n" + await FormatResponse(response);
        }
        catch (Exception ex)
        {
            CustomApiResult = $"Error: {ex.Message}";
        }

        return RedirectToPage();
    }

    public IActionResult OnPostLogout()
    {
        HttpContext.Session.Clear();
        Response.Cookies.Delete("AccessToken");
        return RedirectToPage("/Login");
    }

    private async Task<string> FormatResponse(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(content))
        {
            return $"Status: {(int)response.StatusCode} {response.StatusCode}\nNo content returned";
        }

        try
        {
            var jsonDocument = JsonDocument.Parse(content);
            return JsonSerializer.Serialize(jsonDocument, new JsonSerializerOptions { WriteIndented = true });
        }
        catch
        {
            return content;
        }
    }
}

// Response models
public class RefreshTokenApiResponse
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public RefreshTokenData? Data { get; set; }
}

public class RefreshTokenData
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}