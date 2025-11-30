using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CoffeePeek.Contract.Dtos.Auth;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace CoffeePeek.Web.Pages;

public class LoginModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    : PageModel
{
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

    public bool IsAuthenticated { get; set; }
    public string? UserEmail { get; set; }

    public void OnGet()
    {
        LoadUserData();
        ViewData["IsAuthenticated"] = IsAuthenticated;
        ViewData["UserEmail"] = UserEmail;
        
        // Проверяем, есть ли уже токен
        if (IsAuthenticated)
        {
            Response.Redirect("/Index");
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
            var client = httpClientFactory.CreateClient();
            var apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7001";

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

                if (loginResponse is { IsSuccess: true, Data: not null })
                {
                    HttpContext.Session.SetString("AccessToken", loginResponse.Data.AccessToken);
                    HttpContext.Session.SetString("RefreshToken", loginResponse.Data.RefreshToken);
                    HttpContext.Session.SetString("UserEmail", Email);
                    
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

                    SuccessMessage = "Вход выполнен успешно! Перенаправление...";
                    // Используем Redirect вместо RedirectToPage для более надежного редиректа
                    return Redirect("/Index");
                }
                else
                {
                    ErrorMessage = loginResponse?.Message ?? "Ошибка входа. Пожалуйста, попробуйте снова.";
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                ErrorMessage = $"Ошибка входа: {response.StatusCode}. Пожалуйста, проверьте ваши учетные данные.";
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
    
    public async Task OnPostGoogleLogin()
    {
        await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme,
            new AuthenticationProperties
            {
                RedirectUri = Url.Page("/Login","GoogleResponse")
            });
    }
    
    public async Task<IActionResult> OnGetGoogleResponseAsync()
    {
        var claimRes = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        
        var claims = claimRes.Principal.Identities.FirstOrDefault().Claims.Select(claim => new
        {
            claim.Issuer,
            claim.OriginalIssuer,
            claim.Type,
            claim.Value
        });
        
        var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

        if (!result.Succeeded)
            return Redirect("/Login");

        var idToken = claims.First().Value;

        if (string.IsNullOrEmpty(idToken))
            return Redirect("/Login?error=GoogleAuthFailed");

        var client = httpClientFactory.CreateClient();
        var apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7001";

        var response = await client.PostAsJsonAsync($"{apiBaseUrl}/api/auth/google/login", new { IdToken = idToken });

        if (!response.IsSuccessStatusCode)
            return Redirect("/Login?error=GoogleAuthFailed");

        var resultData = await response.Content.ReadFromJsonAsync<Response<GoogleLoginResponse>>();

        Response.Cookies.Append("auth_token", resultData.Data.AccessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7)
        });

        return Redirect("/");
    }
}

public class LoginApiResponse
{
    [JsonPropertyName("Success")]
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public LoginData? Data { get; set; }
}

public class LoginData
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}