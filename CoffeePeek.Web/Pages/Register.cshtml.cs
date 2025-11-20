using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;
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
                    ErrorMessage = "A user with this email already exists. Please use a different email or login.";
                    return Page();
                }
            }
            
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
                    SuccessMessage = "Registration successful! Redirecting to login...";
                    
                    await Task.Delay(1500);
                    return RedirectToPage("/Login");
                }
                else
                {
                    ErrorMessage = registerResponse?.Message ?? "Registration failed. Please try again.";
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                
                try
                {
                    var errorResponse = JsonSerializer.Deserialize<ApiResponse<RegisterResponse>>(errorContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    ErrorMessage = errorResponse?.Message ?? $"Registration failed: {response.StatusCode}";
                }
                catch
                {
                    ErrorMessage = $"Registration failed: {response.StatusCode}. Please try again.";
                }
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