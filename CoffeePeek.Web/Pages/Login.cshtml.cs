using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoffeePeek.Web.Pages;

public class LoginModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public LoginModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    [BindProperty]
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    public string Password { get; set; } = string.Empty;

    [BindProperty]
    public bool RememberMe { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    [TempData]
    public string? SuccessMessage { get; set; }

    public void OnGet()
    {
        var token = HttpContext.Session.GetString("AccessToken");
        if (!string.IsNullOrEmpty(token))
        {
            Response.Redirect("/Index");
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var client = _httpClientFactory.CreateClient();
            var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7001";

            var loginRequest = new
            {
                email = Email,
                password = Password
            };

            var content = new StringContent(
                JsonSerializer.Serialize(loginRequest),
                Encoding.UTF8,
                "application/json"
            );

            var response = await client.PostAsync($"{apiBaseUrl}/api/Auth/login", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var loginResponse = JsonSerializer.Deserialize<LoginApiResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (loginResponse?.IsSuccess == true && loginResponse.Data != null)
                {
                    // Сохраняем токены в сессии
                    HttpContext.Session.SetString("AccessToken", loginResponse.Data.AccessToken);
                    HttpContext.Session.SetString("RefreshToken", loginResponse.Data.RefreshToken);
                    HttpContext.Session.SetString("UserEmail", Email);
                    
                    // Принудительно сохраняем сессию
                    await HttpContext.Session.CommitAsync();

                    // Если "Remember Me" включен, сохраняем в cookies
                    if (RememberMe)
                    {
                        Response.Cookies.Append("AccessToken", loginResponse.Data.AccessToken, new CookieOptions
                        {
                            Expires = DateTimeOffset.UtcNow.AddDays(30),
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.Strict
                        });
                    }

                    SuccessMessage = "Login successful! Redirecting...";
                    // Используем Redirect вместо RedirectToPage для более надежного редиректа
                    return Redirect("/Index");
                }
                else
                {
                    ErrorMessage = loginResponse?.Message ?? "Login failed. Please try again.";
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                ErrorMessage = $"Login failed: {response.StatusCode}. Please check your credentials.";
            }
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = $"Connection error: Unable to reach the API server. {ex.Message}";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"An error occurred: {ex.Message}";
        }

        return Page();
    }
}

// Response models
public class LoginApiResponse
{
    [JsonPropertyName(("Success"))]
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public LoginData? Data { get; set; }
}

public class LoginData
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}