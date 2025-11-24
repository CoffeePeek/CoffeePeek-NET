using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CoffeePeek.Contract.Response;
using CoffeePeek.Web.Contract.Http.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoffeePeek.Web.Pages;

public class RegisterModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public RegisterModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    [BindProperty]
    [Required(ErrorMessage = "Username is required")]
    [StringLength(30, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 30 characters")]
    public string Username { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Please confirm your password")]
    [Compare("Password", ErrorMessage = "Passwords do not match")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;

    [TempData]
    public string? ErrorMessage { get; set; }

    [TempData]
    public string? SuccessMessage { get; set; }

    public bool IsAuthenticated { get; set; }
    public string? UserEmail { get; set; }

    public void OnGet()
    {
        LoadUserData();
        ViewData["IsAuthenticated"] = IsAuthenticated;
        ViewData["UserEmail"] = UserEmail;
        
        if (!IsAuthenticated)
        {
            var token = HttpContext.Session.GetString("AccessToken");
            if (!string.IsNullOrEmpty(token))
            {
                Response.Redirect("/Index");
            }
        }
    }

    private void LoadUserData()
    {
        var accessToken = HttpContext.Session.GetString("AccessToken") ?? 
                         HttpContext.Request.Cookies["AccessToken"];
        UserEmail = HttpContext.Session.GetString("UserEmail");
        IsAuthenticated = !string.IsNullOrEmpty(accessToken);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        LoadUserData();
        ViewData["IsAuthenticated"] = IsAuthenticated;
        ViewData["UserEmail"] = UserEmail;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var client = _httpClientFactory.CreateClient();
            var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7001";

            // Сначала проверяем, существует ли пользователь с таким email
            var checkResponse = await client.PostAsync(
                $"{apiBaseUrl}/api/Auth/check-exists?email={Uri.EscapeDataString(Email)}", 
                null
            );

            if (checkResponse.IsSuccessStatusCode)
            {
                var checkContent = await checkResponse.Content.ReadAsStringAsync();
                var checkResult = JsonSerializer.Deserialize<CheckExistsResponse>(checkContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (checkResult?.IsSuccess == true)
                {
                    ErrorMessage = "Пользователь с таким email уже существует. Пожалуйста, используйте другой email или войдите.";
                    return Page();
                }
            }

            // Регистрируем нового пользователя
            var registerRequest = new
            {
                userName = Username,
                email = Email,
                password = Password
            };

            var content = new StringContent(
                JsonSerializer.Serialize(registerRequest),
                Encoding.UTF8,
                "application/json"
            );

            var response = await client.PostAsync($"{apiBaseUrl}/api/Auth/register", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var registerResponse = JsonSerializer.Deserialize<CheckExistsResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (registerResponse?.IsSuccess == true)
                {
                    SuccessMessage = "Регистрация выполнена успешно! Перенаправление на страницу входа...";
                    
                    // Небольшая задержка перед редиректом
                    await Task.Delay(1500);
                    return RedirectToPage("/Login");
                }
                else
                {
                    ErrorMessage = registerResponse?.Message ?? "Ошибка регистрации. Пожалуйста, попробуйте снова.";
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                
                // Пытаемся извлечь сообщение об ошибке из ответа
                try
                {
                    var errorResponse = JsonSerializer.Deserialize<Response<RegisterResponse>>(errorContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    ErrorMessage = errorResponse?.Message ?? $"Ошибка регистрации: {response.StatusCode}";
                }
                catch
                {
                    ErrorMessage = $"Ошибка регистрации: {response.StatusCode}. Пожалуйста, попробуйте снова.";
                }
            }
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = $"Ошибка подключения: Не удалось подключиться к серверу API. {ex.Message}";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Произошла ошибка: {ex.Message}";
        }

        return Page();
    }
}